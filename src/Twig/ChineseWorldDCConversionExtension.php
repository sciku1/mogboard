<?php

namespace App\Twig;

use App\Common\Utils\Language;
use Twig\Extension\AbstractExtension;
use Twig\TwigFilter;

class ChineseWorldDCConversionExtension extends AbstractExtension
{
    const MAPPINGS = [
        "LuXingNiao" => "陆行鸟",
        "HongYuHai" => "红玉海",
        "ShenYiZhiDi" => "神意之地",
        "LaNuoXiYa" => "拉诺西亚",
        "HuanYingQunDao" => "幻影群岛",
        "MengYaChi" => "萌芽池",
        "YuZhouHeYin" => "宇宙和音",
        "WoXianXiRan" => "沃仙曦染",
        "ChenXiWangZuo" => "晨曦王座",
        "MoGuLi" => "莫古力",
        "BaiYinXiang" => "白银乡",
        "BaiJinHuanXiang" => "白金幻象",
        "ShenQuanHen" => "神拳痕",
        "ChaoFengTing" => "潮风亭",
        "LvRenZhanQiao" => "旅人栈桥",
        "FuXiaoZhiJian" => "拂晓之间",
        "Longchaoshendian" => "龙巢神殿",
        "MengYuBaoJing" => "梦羽宝境",
        "MaoXiaoPang" => "猫小胖",
        "ZiShuiZhanQiao" => "紫水栈桥",
        "YanXia" => "延夏",
        "JingYuZhuangYuan" => "静语庄园",
        "MoDuNa" => "摩杜纳",
        "HaiMaoChaWu" => "海猫茶屋",
        "RouFengHaiWan" => "柔风海湾",
        "HuPoYuan" => "琥珀原",
    ];

    public function getFilters()
    {
        return [
            new TwigFilter('convert_if_chinese', [$this, 'chinaficateWorldDC']),
        ];
    }
    
    public function chinaficateWorldDC($worldDC)
    {
        if (Language::current() !== 'chs') {
            return $worldDC;
        }
        if (\array_key_exists($worldDC, self::MAPPINGS)){
            return self::MAPPINGS[$worldDC];
        }
        return $worldDC;
    }
}
