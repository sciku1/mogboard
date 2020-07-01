<?php

namespace App\Service\GameData;

use App\Common\Service\Redis\Redis;
use App\Common\Utils\Language;

class CafeMaker {
    private const URLBASE = 'https://cafemaker.wakingsands.com';

    /** @var cURL */
    private $curl;

    public function __construct()
    {
        $this->$curl = curl_init();
    }

    public function getItem(int $itemId)
    {
        $cachedItem = $this->handle("xiv_Item_{$itemId}");
        if ($cachedItem == null) {
            $query = self::URLBASE . "/Item/{$itemId}";
            $cachedItem = $this->getResource($query);
            
            if ($cachedItem != null) {
                Redis::Cache()->set("xiv_Item_{$itemId}", $cachedItem, GameDataCache::CACHE_TIME);
            }
        }

        return json_decode($cachedItem);
    }
    
    public function getRecipe(int $recipeId)
    {
        $cachedRecipe = $this->handle("xiv_Item_{$recipeId}");
        if ($cachedRecipe == null) {
            $query = self::URLBASE . "/Recipe/{$recipeId}";
            $cachedRecipe = $this->getResource($query);
            
            if ($cachedRecipe != null) {
                Redis::Cache()->set("xiv_Recipe_{$recipeId}", $cachedRecipe, GameDataCache::CACHE_TIME);
            }
        }

        return json_decode($cachedRecipe);
    }

    public function getMateria(int $materiaId)
    {
        $cachedMateria = $this->handle("xiv_Materia_{$materiaId}");
        if ($cachedMateria == null) {
            $query = self::URLBASE . "/Materia/{$materiaId}";
            $cachedMateria = $this->getResource($query);
            
            if ($cachedMateria != null) {
                Redis::Cache()->set("xiv_Materia_{$materiaId}", $cachedMateria, GameDataCache::CACHE_TIME);
            }
        }

        return json_decode($res);
    }

    private function getResource(string $url) {
        curl_setopt_array($this->$curl, [
            CURLOPT_URL => $url
        ]);
        return curl_exec($this->$curl);
    }

    private function handle(string $key)
    {
        return Language::handle(
            Redis::Cache()->get($key)
        );
    }
}