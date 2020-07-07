<?php

namespace App\Command\I18n;

use App\Command\CommandConfigureTrait;
use App\Service\I18n\TranslationDataCache;
use Symfony\Component\Console\Command\Command;
use Symfony\Component\Console\Input\InputInterface;
use Symfony\Component\Console\Output\OutputInterface;

class ImportTranslationsCommand extends Command
{
    use CommandConfigureTrait;

    const COMMAND = [
        'name' => 'ImportTranslationsCommand',
        'desc' => 'Imports translations from the translations folder.',
    ];

    /**
     * @var TranslationDataCache
     */
    private $tdc;

    public function __construct(TranslationDataCache $tdc, string $name = null)
    {
        $this->tdc = $tdc;

        parent::__construct($name);
    }
    
    protected function execute(InputInterface $input, OutputInterface $output)
    {
        $this->tdc->populate();
    }
}
