<?php

namespace App\Twig;

use App\Common\Game\GameServers;
use App\Service\Companion\CompanionCensus;
use App\Service\Companion\CompanionMarket;
use Twig\Extension\AbstractExtension;
use Twig\TwigFilter;
use Twig\TwigFunction;

// I eventually want to move a bunch of things to extensions, it decouples interfaces from controllers and makes it easier to add new things and fix broken things
class MarketExtension extends AbstractExtension
{
    /** @var CompanionCensus */
    private $companionCensus;
    /** @var CompanionMarket */
    private $companionMarket;

    public function __construct(CompanionCensus $companionCensus, CompanionMarket $companionMarket)
    {
        $this->companionCensus = $companionCensus;
        $this->companionMarket = $companionMarket;
    }

    public function getFunctions()
    {
        return [
            new TwigFunction('market', [$this, 'getMarket']),
            new TwigFunction('census', [$this, 'getCensus']),
            new TwigFunction('updateTimes', [$this, 'getUpdateTimes']),
            new TwigFunction('lastUpdateTime', [$this, 'getLastUpdateTimesDC']),
        ];
    }

    public function getMarket($itemId, $server = null)
    {
        $world = $server ?? GameServers::getServer();
        $dc = GameServers::getDataCenter($world);
        $dcServers = GameServers::getDataCenterServers($world);
        $market = $this->companionMarket->get($dcServers, $itemId);
        return $market;
    }
    
    public function getCensus($itemId, $server = null)
    {
        $world = $server ?? GameServers::getServer();
        $dc = GameServers::getDataCenter($world);
        $dcServers = GameServers::getDataCenterServers($world);
        $market = $this->companionMarket->get($dcServers, $itemId);
        $census = $this->companionCensus->generate($dc, $itemId, $market);
        return $census;
    }

    public function getUpdateTimes($itemId, $server = null)
    {
        $market = $this->getMarket($itemId, $server);

        $times = [];
        foreach ($market as $marketServer => $md) {
            if ($md == null) {
                continue;
            }

            $times[] = [
                'name'     => $marketServer,
                'updated'  => $md['lastUploadTime']
            ];
        }

        return $times;
    }

    public function getLastUpdateTimesDC($itemId, $server = null)
    {
        $times = $this->getUpdateTimes($itemId, $server);
        $lastUpdated = 0;

        foreach ($times as $entry) {
            if ($entry['updated'] > $lastUpdated) {
                $lastUpdated = $entry['updated'];
            }
        }

        return $lastUpdated;
    }
}
