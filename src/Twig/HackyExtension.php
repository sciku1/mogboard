<?php // Everything in this file should be moved someplace more appropriate, if things are here it's only because it was too much of a pain to put it in the right place.

namespace App\Twig;

use App\Common\Utils\Language;
use Twig\Extension\AbstractExtension;
use Twig\TwigFilter;

class HackyExtension extends AbstractExtension
{
    public function getFilters()
    {
        return [
            new TwigFilter('chinaizeisc', [$this, 'chinaizeSearchCategories']),
        ];
    }
    
    // This is here because getting the cache method to load Chinese data is proving to be a major pain.
    // The data is being cached correctly (I think), just not converted properly.
    public function chinaizeSearchCategories($obj)
    {
        if (Language::current() !== 'chs') {
            return $obj;
        }
        
        $retObj = [
            'weapons' => [],
            'armor' => [],
            'items' => [],
            'housing' => [],
        ];
        $weaponsCatIds = [10, 11, 76, 86, 13, 9, 83, 73, 12, 77, 87, 14, 16, 84, 15, 85, 78, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30];
        $armorCatIds = [17, 31, 33, 36, 38, 35, 37, 40, 39, 41, 42];
        $itemsCatIds = [43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 57, 58, 59, 60, 74, 75, 79, 80];
        $housingCatIds = [65, 66, 67, 56, 68, 69, 70, 71, 72, 81, 82];
        $data = json_decode(file_get_contents('../DataExports/ItemSearchCategory_Mappings_Chs.json'), TRUE);
        foreach ($weaponsCatIds as $i) {
            array_push($retObj['weapons'], [
                'ID' => ''.$i,
                'Name' => $data[$i],
            ]);
        }
        foreach ($armorCatIds as $i) {
            array_push($retObj['armor'], [
                'ID' => ''.$i,
                'Name' => $data[$i],
            ]);
        }
        foreach ($itemsCatIds as $i) {
            array_push($retObj['items'], [
                'ID' => ''.$i,
                'Name' => $data[$i],
            ]);
        }
        foreach ($housingCatIds as $i) {
            array_push($retObj['housing'], [
                'ID' => ''.$i,
                'Name' => $data[$i],
            ]);
        }
        return $retObj;
    }
}
