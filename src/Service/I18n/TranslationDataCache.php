<?php

namespace App\Service\I18n;

use App\Common\Service\Redis\Redis;
use Symfony\Component\Console\Output\ConsoleOutput;

class TranslationDataCache
{
    // cache to 2030
    const CACHE_TIME = 1890691200;

    /** @var ConsoleOutput */
    private $console;

    public function __construct()
    {
        $this->console = new ConsoleOutput();
    }

    public function populate() {
        $this->cacheTranslations();
    }
    
    private function cacheTranslations()
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
                if ($this->isNullOrEmpty($term->definition)) {
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
