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
        if (Language::current() === 'chs') {
            $retObj = [
                'weapons' => [],
                'armor' => [],
                'items' => [],
                'housing' => [],
            ];
            $data = json_decode(file_get_contents('../DataExports/ItemSearchCategory_Mappings_Chs.json'), TRUE);
            foreach ($data as $i => $isc) {
                if (in_array($i, [10, 11, 76, 86, 13, 9, 83, 73, 12, 77, 87, 14, 16, 84, 15, 85, 78, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30])) {
                    array_push($retObj['weapons'], [
                        'ID' => ''.$i,
                        'Name' => $isc,
                    ]);
                } else if (in_array($i, [17, 31, 33, 36, 38, 35, 37, 40, 39, 41, 42])) {
                    array_push($retObj['armor'], [
                        'ID' => ''.$i,
                        'Name' => $isc,
                    ]);
                } else if ($i >= 43 and $i <= 60 and $i !== 56 or in_array($i, [74, 75, 79, 80])) {
                    array_push($retObj['items'], [
                        'ID' => ''.$i,
                        'Name' => $isc,
                    ]);
                } else if (in_array($i, [65, 66, 67, 56, 68, 69, 70, 71, 72, 81, 82])) {
                    array_push($retObj['housing'], [
                        'ID' => ''.$i,
                        'Name' => $isc,
                    ]);
                }
            }
            return $retObj;
        }
        return $obj;
    }
}
