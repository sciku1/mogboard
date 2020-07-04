<?php

namespace App\Service\GameData;

use App\Common\Service\Redis\Redis;
use App\Common\Utils\Language;

class CafeMaker {
    private const URLBASE = 'https://cafemaker.wakingsands.com';

    public function __construct()
    {
    }

    public function getItem(int $itemId)
    {
        /*$cachedItem = Redis::Cache()->get("xiv_Item_Chs_{$itemId}");
        if ($cachedItem == null) {*/
            $query = self::URLBASE . "/Item/{$itemId}";
            $cachedItem = $this->getResource($query);
            
            if ($cachedItem != null) {
                Redis::Cache()->set("xiv_Item_Chs_{$itemId}", $cachedItem, GameDataCache::CACHE_TIME);
            }
        //}

        return json_decode($cachedItem);
    }
    
    public function getRecipe(int $recipeId)
    {
        $cachedRecipe = Redis::Cache()->get("xiv_Item_Chs_{$recipeId}");
        if ($cachedRecipe == null) {
            $query = self::URLBASE . "/Recipe/{$recipeId}";
            $cachedRecipe = $this->getResource($query);
            
            if ($cachedRecipe != null) {
                Redis::Cache()->set("xiv_Recipe_Chs_{$recipeId}", $cachedRecipe, GameDataCache::CACHE_TIME);
            }
        }

        return json_decode($cachedRecipe);
    }

    public function getMateria(int $materiaId)
    {
        $cachedMateria = Redis::Cache()->get("xiv_Materia_Chs_{$materiaId}");
        if ($cachedMateria == null) {
            $query = self::URLBASE . "/Materia/{$materiaId}";
            $cachedMateria = $this->getResource($query);
            
            if ($cachedMateria != null) {
                Redis::Cache()->set("xiv_Materia_Chs_{$materiaId}", $cachedMateria, GameDataCache::CACHE_TIME);
            }
        }

        return json_decode($cachedMateria);
    }

    private function getResource(string $url): String {
        $curl = curl_init();
        curl_setopt_array($curl, [
            CURLOPT_URL => $url,
            CURLOPT_RETURNTRANSFER => TRUE,
        ]);
        $res = curl_exec($curl);
        curl_close($curl);
        return $res;
    }
}