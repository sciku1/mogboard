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
            new TwigFunction('census', [$this, 'getCensus']),
        ];
    }
    
    public function getCensus($itemId)
    {
        $world = GameServers::getServer();
        $dc = GameServers::getDataCenter($world);
        $dcServers = GameServers::getDataCenterServers($dc);
        $market = $this->companionMarket->get($dcServers, $itemId);
        return $this->companionCensus->generate($dc, $itemId, $market);
    }
}
