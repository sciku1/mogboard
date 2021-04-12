<?php

namespace App\Service\UserAlerts;

use App\Common\Entity\UserAlert;
use App\Common\Service\Redis\Redis;
use Carbon\Carbon;

class UserAlertsEmailNotification
{
    const ALERTS_SERVICE_ENDPOINT = "http://localhost:7584/email/send";
    
    /**
     * Send a notification regarding triggers
     */
    public function sendAlertTriggerNotification(UserAlert $alert, array $triggeredMarketRows, string $hash)
    {
        $item = Redis::Cache()->get("xiv_Item_{$alert->getItemId()}");

        $reasons = [];
        foreach($triggeredMarketRows as $i => $marketRow) {
            [$server, $row] = $marketRow;

            $name = sprintf(
                "%sx %s Gil - Total: %s",
                number_format($row->Quantity),
                number_format($row->PricePerUnit),
                number_format($row->PriceTotal),
                $row->IsHQ ? "HQ" : "NQ",
                $alert->getTriggerType() == "Prices" ? $row->RetainerName : $row->CharacterName
            );

            $purchaseDate = null;
            if ($alert->getTriggerType() == "History") {
                $carbon = Carbon::createFromTimestamp($row->PurchaseDate);
                $purchaseDate = $carbon->fromNow();
            }

            $value = sprintf(
                "%s - %s - %s",
                "({$server})",
                $alert->getTriggerType() == "Prices" ? "Retainer: {$row->RetainerName}" : "Buyer: {$row->CharacterName}",
                $alert->getTriggerType() == "Prices" ? "Signature: {$row->CreatorSignatureName}" : "Purchased: {$purchaseDate}"
            );

            $reasons[$i] = $name . " " . $value;
        }

        $notificationInfo = [
            "itemName"      => $item->Name_en,
            "itemIcon"      => "https://xivapi.com{$item->Icon}",
            "pageUrl"       => getenv("SITE_CONFIG_DOMAIN") . "/market/{$item->ID}",
            "reasons"       => $reasons,
        ];

        postJsonResource(ALERTS_SERVICE_ENDPOINT, [
            "targetUser"   => "", // TODO
            "notification" => $notificationInfo,
        ]);
    }

    private function postJsonResource(string $url, array $postData): string {
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
