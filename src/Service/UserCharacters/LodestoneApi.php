<?php

namespace App\Service\UserCharacters;

class LodestoneApi {
    private const URLBASE = 'http://localhost:3999/lodestone';

    public function __construct()
    {
    }

    public function getCharacter(int $lodestoneId)
    {
        $res = $this->getResource("/character/{$lodestoneId}");
        return json_decode($res);
    }

    private function getResource(string $url): string {
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