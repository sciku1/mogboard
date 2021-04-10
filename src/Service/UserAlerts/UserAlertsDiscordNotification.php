<?php

namespace App\Service\UserAlerts;

use App\Common\Entity\UserAlert;
use App\Common\Service\Redis\Redis;

class UserAlertsDiscordNotification
{
    const ALERTS_SERVICE_ENDPOINT = "http://localhost:7584/discord/send";
    
    /**
     * Send a notification regarding triggers
     */
    public function sendAlertTriggerNotification(UserAlert $alert, array $triggeredMarketRows, string $hash)
    {
        $item = Redis::Cache()->get("xiv_Item_{$alert->getItemId()}");

        $notificationInfo = [
            "itemName"      => $item->Name_en,
            "itemIcon"      => "https://xivapi.com{$item->Icon}",
            "pageUrl"       => getenv("SITE_CONFIG_DOMAIN") . "/market/{$item->ID}",
            "reasons"       => [], // TODO
        ];
        
        postResource(ALERTS_SERVICE_ENDPOINT, [
            'targetUser'   => $alert->getUser()->getSsoDiscordId(),
            'notification' => $notificationInfo,
        ]);
    }

    private function postResource(string $url, array $postData): string {
        $postDataStr = json_encode($postData);
        $curl = curl_init();
        curl_setopt_array($curl, [
            CURLOPT_URL            => $url,
            CURLOPT_RETURNTRANSFER => TRUE,
            CURLINFO_HEADER_OUT    => TRUE,
            CURLOPT_POST           => TRUE,
            CURLOPT_POSTFIELDS     => $postDataStr,
            CURLOPT_HTTPHEADER     => array(
                "Content-Type: application/json",
                "Content-Length: " . strlen($postDataStr),
            ),
        ]);
        $res = curl_exec($curl);
        curl_close($curl);
        return $res;
    }
}
