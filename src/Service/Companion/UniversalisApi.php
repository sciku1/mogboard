<?php

namespace App\Service\Companion;

use GuzzleHttp\Client;
use GuzzleHttp\RequestOptions;

class UniversalisApi
{
    const PROD    = 'http://localhost:4000';
    const STAGING = 'http://localhost:4001';

    const TIMEOUT = 10.0;
    const VERIFY = false;

    /** @var Client */
    private $client = null;

    public function __construct(string $environment = self::PROD)
    {
        $this->client = new Client([
            'base_uri'  => $environment,
            'timeout'   => self::TIMEOUT,
            'verify'    => self::VERIFY,
        ]);
    }

    public function getItem(int $worldId, int $itemId)
    {
        return $this->query("GET", "api/{$worldId}/{$itemId}", [
            RequestOptions::QUERY => [
                'src'   => 'universalis_front'
            ]
        ]);
    }

    public function getItemFromWorldOrDC(String $worldIdOrDCName, int $itemId)
    {
        return $this->query("GET", "api/{$worldIdOrDCName}/{$itemId}", [
            RequestOptions::QUERY => [
                'src'   => 'universalis_front'
            ]
        ]);
    }

    public function getExtendedHistory(int $worldId, int $itemId, int $numEntries = 200)
    {
        return $this->query("GET", "api/{$worldId}/{$itemId}", [
            RequestOptions::QUERY => [
                'src'   => 'universalis_front'
            ]
        ]);
    }

    public function getRecentlyUpdated()
    {
        return $this->query("GET", "api/extra/stats/recently-updated", [
            RequestOptions::QUERY => [
                'src'   => 'universalis_front'
            ]
        ]);
    }

    public function getLeastRecentlyUpdated(int $worldId)
    {
        return $this->query("GET", "api/extra/stats/least-recently-updated", [
            RequestOptions::QUERY => [
                'world' => $worldId,
                'src'   => 'universalis_front'
            ]
        ]);
    }

    public function getDCLeastRecentlyUpdated(string $dcName)
    {
        return $this->query("GET", "api/extra/stats/least-recently-updated", [
            RequestOptions::QUERY => [
                'dcName' => $dcName,
                'src'   => 'universalis_front'
            ]
        ]);
    }


    public function getUploadHistory()
    {
        return $this->query("GET", "api/extra/stats/upload-history", [
            RequestOptions::QUERY => [
                'src'   => 'universalis_front'
            ]
        ]);
    }

    public function getWorldUploadCounts()
    {
        return $this->query("GET", "api/extra/stats/world-upload-counts", [
            RequestOptions::QUERY => [
                'src'   => 'universalis_front'
            ]
        ]);
    }

    public function getUploaderUploadCounts()
    {
        return $this->query("GET", "api/extra/stats/uploader-upload-counts", [
            RequestOptions::QUERY => [
                'src'   => 'universalis_front'
            ]
        ]);
    }

    public function getTaxRates(int $worldId)
    {
        return $this->query("GET", "api/tax-rates", [
            RequestOptions::QUERY => [
                'world' => $worldId,
                'src'   => 'universalis_front'
            ]
        ]);
    }

    private function query($method, $apiEndpoint, $options = [])
    {
        // set XIVAPI key
        /*
        if ($key = getenv(Environment::XIVAPI_KEY)) {
            $options[RequestOptions::QUERY]['private_key'] = $key;
        }
        */

        return \GuzzleHttp\json_decode(
            $this->client->request($method, $apiEndpoint, $options)->getBody()
        );
    }
}
