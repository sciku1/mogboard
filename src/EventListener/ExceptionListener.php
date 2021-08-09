<?php

namespace App\EventListener;

use App\Common\Constants\DiscordConstants;
use App\Common\Exceptions\BasicException;
use App\Common\Service\Redis\Redis;
use App\Common\ServicesThirdParty\Discord\Discord;
use App\Common\Utils\Environment;
use App\Common\Exceptions\JsonException;
use Symfony\Component\EventDispatcher\EventSubscriberInterface;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\HttpKernel\Event\GetResponseForExceptionEvent;
use Symfony\Component\HttpKernel\Exception\NotFoundHttpException;
use Symfony\Component\HttpKernel\KernelEvents;
use Twig\Environment as TwigEnvironment;

class ExceptionListener implements EventSubscriberInterface
{
    /** @var TwigEnvironment */
    private $twig;

    /**
     * TestTwig constructor.
     */
    public function __construct(TwigEnvironment $twig)
    {
        $this->twig = $twig;
    }

    public static function getSubscribedEvents()
    {
        return [
            KernelEvents::EXCEPTION => 'onKernelException'
        ];
    }

    /**
     * Handle custom exceptions
     * @param GetResponseForExceptionEvent $event
     * @return null|void
     */
    public function onKernelException(GetResponseForExceptionEvent $event)
    {
        /**
         * If we're in dev mode, show the full error
         */
        if (getenv('APP_ENV') == 'dev') {
            return null;
        }

        /**
         * Make sure it isn't an image or some kind of file
         */
        $pi = pathinfo(
            $event->getRequest()->getPathInfo()
        );

        if (isset($pi['extension']) && strlen($pi['extension'] > 2)) {
            $event->setResponse(new Response("File not found, sorry. Try harder.", 404));
            return null;
        }

        /**
         * Handle error info
         */
        $ex    = $event->getException();
        $error = (Object)[
            'site'          => 'MOGBOARD',
            'message'       => $ex->getMessage() ?: '(no-exception-message)',
            'code'          => $ex->getCode() ?: 500,
            'ex_class'      => get_class($ex),
            'ex_file'       => $ex->getFile(),
            'ex_line'       => $ex->getLine(),
            'req_uri'       => $event->getRequest()->getUri(),
            'req_method'    => $event->getRequest()->getMethod(),
            'req_path'      => $event->getRequest()->getPathInfo(),
            'req_action'    => $event->getRequest()->attributes->get('_controller'),
            'env'           => constant(Environment::CONSTANT),
            'date'          => date('Y-m-d H:i:s'),
            'hash'          => sha1($ex->getMessage() . $ex->getFile()),
        ];

        /**
         * Send error to discord if not sent within the hour AND the exception is not a valid one.
         */
        $validExceptions = [
            BasicException::class,
            NotFoundHttpException::class,
            JsonException::class,
        ];

        $madeAware = false;
        if (Redis::Cache()->get(__METHOD__ . $error->hash) == null && !in_array($error->ex_class, $validExceptions) && $error->env != 'local') {
            Redis::Cache()->set(__METHOD__ . $error->hash, true);
            $madeAware = true;
            Discord::mog()->sendMessage(
                DiscordConstants::ROOM_ERRORS,
                "```json\n". json_encode($error, JSON_PRETTY_PRINT) ."\n```"
            );
        }

        /**
         * If it's a json exception
         */
        if ($error->ex_class === JsonException::class) {
            $response = new JsonResponse([
                'Error'    => true,
                'Message'  => $ex->getMessage(),
                'Hash'     => sha1($ex->getMessage() . $ex->getFile()),
                'ExClass'  => get_class($ex),
            ], $error->code);
            $response->headers->set('Content-Type','application/json');
            $response->headers->set('Access-Control-Allow-Origin', '*');
            $response->headers->set('Access-Control-Allow-Headers', '*');
            $event->setResponse($response);
            return;
        }

        /**
         * Render error to the user
         */
        $html = $this->twig->render('Errors/error.html.twig', [ 'error' => $error, 'made_aware' => $madeAware ]);
        $response = new Response($html, $error->code);
        $event->setResponse($response);
    }
}
