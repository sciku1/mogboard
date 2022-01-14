<?php

namespace App\Controller;

use App\Common\Game\GameServers;
use App\Common\ServicesThirdParty\Discord\Discord;
use App\Common\User\Users;
use App\Common\Utils\Mail;
use App\Service\Companion\CompanionMarketActivity;
use App\Service\Companion\CompanionStatistics;
use App\Service\Companion\UniversalisApi;
use App\Service\Items\Popularity;
use App\Common\Service\Redis\Redis;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\Request;
use Symfony\Component\Routing\Annotation\Route;
use XIVAPI\XIVAPI;

class IndexController extends AbstractController
{
    /** @var Popularity */
    private $itemPopularity;
    /** @var CompanionStatistics */
    private $companionStatistics;
    /** @var CompanionMarketActivity */
    private $companionMarketActivity;
    /** @var UniversalisApi */
    private $universalisApi;
    /** @var Users */
    private $users;
    /** @var Mail */
    private $mail;

    public function __construct(
        Popularity $itemPopularity,
        CompanionStatistics $companionStatistics,
        CompanionMarketActivity $companionMarketActivity,
        Users $users,
        Mail $mail
    ) {
        $this->itemPopularity           = $itemPopularity;
        $this->companionStatistics      = $companionStatistics;
        $this->companionMarketActivity  = $companionMarketActivity;
        $this->users                    = $users;
        $this->mail                     = $mail;
        $this->universalisApi           = new UniversalisApi();
    }

    /**
     * @Route("/", name="home")
     */
    public function home(Request $request)
    {
        $this->users->setLastUrl($request);

        // grab the users market feed
        //$marketFeed = $this->companionMarketActivity->getFeed($this->users->getUser());
        //$marketFeed = json_decode(json_encode($marketFeed), true);

        $uploads = $this->universalisApi->getUploadHistory();
        $uploads = json_decode(json_encode($uploads), true);

        $recentUpdates = $this->universalisApi->getRecentlyUpdated();
        $recentUpdates = json_decode(json_encode($recentUpdates), true);

        // Pie chart for world upload counts
        $uploadsWorld = $this->universalisApi->getWorldUploadCounts();
        $uploadsWorld = json_decode(json_encode($uploadsWorld), true);
        $pieData = [];
        foreach ($uploadsWorld as $key => $value) {
            $nextItem = new \stdClass();
            $nextItem->name = $key;
            $nextItem->y = $value['count'];
            \array_push($pieData, $nextItem);
        }

        $renderParameters = [
        //    'market_feed'   => $marketFeed,
            'popular_items' => $this->itemPopularity->get(),
            'uploads_today' => sizeof($uploads['uploadCountByDay']) > 0 ? $uploads['uploadCountByDay'][0] : 0,
            'uploads_week'  => \array_sum($uploads['uploadCountByDay']),
            'uploads_world' => $pieData,
            'recent'        => \array_slice($recentUpdates['items'], 0, 6)
        ];

        // Grab server info
        $server = GameServers::getServer($request->get('server'));

        if ($server != null) {
            $renderParameters['server'] = $server;

            $server = GameServers::getServerId($server);

            // Get tax rates on this server
            $taxRates = $this->universalisApi->getTaxRates($server);
            $taxRates = json_decode(json_encode($taxRates), true);

            $renderParameters['tax_rates'] = $taxRates;
        }

        return $this->render('Home/home.html.twig', $renderParameters);
    }

    /**
     * @Route("/.well-known/acme-challenge/{hash}")
     */
    public function le($hash)
    {
        return $this->json($hash);
    }

    /**
     * @Route("/404", name="404")
     */
    public function fourOfour()
    {
        return $this->render('Errors/404.html.twig');
    }

    /**
     * @Route("/error", name="error")
     */
    public function error()
    {
        throw new \Exception("This is a test error");
    }

    /**
     * @Route("/news", name="news_index")
     * @Route("/news/{slug}", name="news")
     */
    public function news(?string $slug = null)
    {
        $templates = [
            'universalis_launch'                 => '2019_09_09.html.twig',
        ];

        $slug = $slug ?: 'universalis_launch';

        return $this->render('News/'. $templates[$slug]);
    }

    /**
     * @Route("/patreon", name="patreon")
     */
    public function patreon()
    {
        return $this->render('Pages/patreon.html.twig', [
            'user_patrons' => $this->users->getPatrons()
        ]);
    }

    /**
     * @Route("/patreon/refund", name="patreon_refund")
     */
    public function patreonRefund()
    {
        return $this->render('Pages/patreon_refund.html.twig');
    }

    /**
     * @Route("/patreon/refund/request", name="patreon_refund_process")
     */
    public function patreonRefundProcess(Request $request)
    {

        return $this->redirectToRoute('patreon_refund', [
            'complete' => 1
        ]);
    }

    /**
     * @Route("/feedback", name="feedback")
     */
    public function feedback(Request $request)
    {
        $sent = $request->getSession()->get('feedback_sent');
        $request->getSession()->remove('feedback_sent');

        return $this->render('Pages/feedback.html.twig', [
            'feedback_sent' => $sent
        ]);
    }

    /**
     * @Route("/feedback/send", name="feedback_send")
     */
    public function feedbackSubmit(Request $request)
    {
        $message = trim($request->get('feedback_message'));
        $message = substr($message, 0, 1000);
        $user    = $this->users->getUser(false);

        $request->getSession()->set('feedback_sent', 'no');

        if (strtolower($request->get('ted')) !== 'ffxiv') {
            return $this->redirectToRoute('feedback');
        }

        if (strtoupper($request->get('gil')) !== 'NO') {
            return $this->redirectToRoute('feedback');
        }

        if (strlen($message) == 0) {
            return $this->redirectToRoute('feedback');
        }

        $key   = 'mb_feedback_client_'. md5($request->getClientIp());
        $count = Redis::Cache()->get($key) ?: 0;
        $count = $count + 1;

        if ($count > 10) {
            return $this->redirectToRoute('feedback');
        }

        Redis::Cache()->set($key, $count);
        $request->getSession()->set('feedback_sent', 'yes');

        $embed = [
            'title'         => "Mogboard Feedback",
            'description'   => $message,
            'color'         => hexdec('c588f7'),
            'fields'        => [
                [
                    'name'   => 'User',
                    'value'  => $user ? "{$user->getUsername()} ({$user->getEmail()})" : "Not online",
                    'inline' => true,
                ]
            ],
        ];

        Discord::mog()->sendMessage('574593645626523669', null, $embed);

        return $this->redirectToRoute('feedback');
    }

    /**
     * @Route("/about", name="about")
     */
    public function about()
    {

        $stats = $this->universalisApi->getUploadHistory();
        $stats = json_decode(json_encode($stats), true);

        $stats = [];
        return $this->render('Pages/about.html.twig', [
            'market_stats' => $stats,
        ]);
    }

    /**
     * @Route("/contribute", name="contribute")
     */
    public function contribute()
    {
        /*
        $stats = $this->companionStatistics->stats();
        $stats = json_decode(json_encode($stats), true);

        */
        $stats = [];
        return $this->render('Pages/contribute.html.twig', [
            'market_stats' => $stats,
        ]);
    }

    /**
     * @Route("/server-status", name="server_status")
     */
    public function serverStatus()
    {
        $xivapi  = new XIVAPI();
        $status  = $xivapi->market->online();
        $list    = [];

        foreach ($status->Status as $i => $serverStatus) {
            $list[$serverStatus->Server] = $serverStatus;
        }

        return $this->render('Pages/servers.html.twig',[
            'servers_status'  => $list
        ]);
    }
}
