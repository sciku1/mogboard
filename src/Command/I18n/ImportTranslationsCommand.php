<?php

namespace App\Command\I18n;

use App\Command\CommandConfigureTrait;
use App\Common\Service\Redis\Redis;
use Symfony\Component\Console\Command\Command;
use Symfony\Component\Console\Input\InputInterface;
use Symfony\Component\Console\Output\OutputInterface;

class ImportTranslationsCommand extends Command
{
    use CommandConfigureTrait;

    // cache to 2030
    const CACHE_TIME = 1890691200;

    const COMMAND = [
        'name' => 'ImportTranslationsCommand',
        'desc' => 'Imports translations from the translations folder.',
    ];

    public function __construct($name = null)
    {
        parent::__construct($name);
    }
    
    protected function execute(InputInterface $input, OutputInterface $output)
    {
        $translationPaths = [
            'chs' => 'translations/Universalis_Chinese_simplified.json',
            'ja' => 'translations/Universalis_Japanese.json',
            'fr' => 'translations/Universalis_French.json',
            'de' => 'translations/Universalis_German.json',
        ];
        
        foreach ($translationPaths as $lang => $path) {
            if (!\file_exists($path)) {
                continue;
            }

            $translationSet = \json_decode(\file_get_contents($path));

            $this->console->writeln('>> Caching translations ('.$lang.')');

            foreach ($translationSet as $term) {
                if (isNullOrEmpty($term)) {
                    continue;
                }

                Redis::Cache()->set('translation_'.$term->context.'_'.$lang, $term->definition, self::CACHE_TIME);
            }
        }

        $this->console->writeln('Done!');
    }

    private function isNullOrEmpty($str) {
        return (!isset($str) || trim($str) === '');
    }
}
