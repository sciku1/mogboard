<?php

namespace App\Twig;

use App\Common\Service\Redis\Redis;
use App\Common\Utils\Language;
use Twig\Extension\AbstractExtension;
use Twig\TwigFilter;

class TranslationExtension extends AbstractExtension {
    public function getFilters()
    {
        return [
            new TwigFilter('translate', [$this, 'translate']),
        ];
    }

    /**
     * Pulls a translation from the translation store using the provided key, falling back to the input text
     * if no data is available.
     */
    public function translate($text, $key): string
    {
        $lang = Language::current();
        $result = Redis::Cache()->get('translation_'.$key.'_'.$lang);
        return $this->isNullOrEmpty($result) ? $text : $result;
    }

    private function isNullOrEmpty($str) {
        return (!isset($str) || trim($str) === '');
    }
}