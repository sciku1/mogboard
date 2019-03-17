<?php

namespace App\Entity;

use Ramsey\Uuid\Uuid;
use Doctrine\ORM\Mapping as ORM;
use Symfony\Component\HttpFoundation\Request;

/**
 * @ORM\Table(name="users_alerts")
 * @ORM\Entity(repositoryClass="App\Repository\AlertRepository")
 */
class UserAlert
{
    const TRIGGERS = [
        // price per unit
        100 => 'Price Per Unit > [Condition]',
        110 => 'Price Per Unit < [Condition]',
        120 => 'Price Per Unit = [Condition]',
        130 => 'Price Per Unit Avg > [Condition]',
        140 => 'Price Per Unit Avg < [Condition]',

        200 => 'Price Total > [Condition]',
        210 => 'Price Total < [Condition]',
        220 => 'Price Total = [Condition]',
        230 => 'Price Total Avg > [Condition]',
        240 => 'Price Total Avg < [Condition]',

        300 => 'Single Stock Quantity > [Condition]',
        310 => 'Single Stock Quantity < [Condition]',
        320 => 'Single Stock Quantity = [Condition]',

        400 => 'Total Stock Quantity > [Condition]',
        410 => 'Total Stock Quantity < [Condition]',
        420 => 'Total Stock Quantity = [Condition]',

        #500 => 'Total Stock Quantity > [Condition]',
        #510 => 'Total Stock Quantity < [Condition]',
        #520 => 'Total Stock Quantity = [Condition]',
        
        600 => 'Retainer Name = [Condition]',
        700 => 'Buyer Name = [Condition]',
        800 => 'Craft Name = [Condition]'
    ];
    const LIMIT_DEFAULT = 5;
    const DELAY_DEFAULT = 300;

    /**
     * @var string
     * @ORM\Id
     * @ORM\Column(type="guid")
     */
    private $id;
    /**
     * @var User
     * @ORM\ManyToOne(targetEntity="User", inversedBy="alerts")
     * @ORM\JoinColumn(name="user_id", referencedColumnName="id")
     */
    private $user;
    /**
     * @var int
     * @ORM\Column(type="integer")
     */
    private $itemId;
    /**
     * @var int
     * @ORM\Column(type="integer")
     */
    private $added;
    /**
     * @var string
     * @ORM\Column(type="string", length=100)
     */
    private $name;
    /**
     * @var string
     * @ORM\Column(type="string", length=100)
     */
    private $server;
    /**
     * @var boolean
     * @ORM\Column(type="boolean", options={"default": false})
     */
    private $triggerDataCenter = false;
    /**
     * @var int
     * @ORM\Column(type="integer", length=3)
     */
    private $triggerOption;
    /**
     * @var string
     * @ORM\Column(type="string", length=64)
     */
    private $triggerValue;
    /**
     * @var int
     * @ORM\Column(type="integer")
     */
    private $triggerLimit = self::LIMIT_DEFAULT;
    /**
     * @var int
     * @ORM\Column(type="integer")
     */
    private $triggerDelay = self::DELAY_DEFAULT;
    /**
     * @var int
     * @ORM\Column(type="integer")
     */
    private $triggerLastSent = 0;
    /**
     * @var boolean
     * @ORM\Column(type="boolean", options={"default": false})
     */
    private $triggerHq = false;
    /**
     * @var boolean
     * @ORM\Column(type="boolean", options={"default": false})
     */
    private $triggerNq = false;
    /**
     * @var boolean
     * @ORM\Column(type="boolean", options={"default": true})
     */
    private $triggerActive = true;
    /**
     * @var boolean
     * @ORM\Column(type="boolean", options={"default": false})
     */
    private $notifiedViaEmail = false;
    /**
     * @var boolean
     * @ORM\Column(type="boolean", options={"default": false})
     */
    private $notifiedViaDiscord = false;
    
    public function __construct()
    {
        $this->id = Uuid::uuid4();
        $this->added = time();
    }

    /**
     * Build a new alert from a json payload request.
     */
    public static function buildFromRequest(Request $request, ?UserAlert $alert = null): UserAlert
    {
        $obj = \GuzzleHttp\json_decode($request->getContent());

        $alert = $alert ?: new UserAlert();

        return $alert
            ->setItemId($obj->itemId ?: $alert->getItemId())
            ->setName($obj->name ?: $alert->getName())
            ->setTriggerDataCenter($obj->dc ?: $alert->isTriggerDataCenter())
            ->setTriggerOption($obj->option ?: $alert->getTriggerOption())
            ->setTriggerValue($obj->value ?: $alert->getTriggerValue())
            ->setTriggerHq($obj->hq ?: $alert->isTriggerHq())
            ->setTriggerNq($obj->nq ?: $alert->isTriggerNq())
            ->setNotifiedViaDiscord($obj->discord ?: $alert->isNotifiedViaDiscord())
            ->setNotifiedViaEmail($obj->email ?: $alert->isNotifiedViaEmail());
    }

    public function getTrigger()
    {
        return self::TRIGGERS[$this->getTriggerOption()];
    }
    
    public function getId(): string
    {
        return $this->id;
    }
    
    public function setId(string $id)
    {
        $this->id = $id;
        
        return $this;
    }
    
    public function getUser(): User
    {
        return $this->user;
    }
    
    public function setUser(User $user)
    {
        $this->user = $user;
        
        return $this;
    }
    
    public function getItemId(): int
    {
        return $this->itemId;
    }
    
    public function setItemId(int $itemId)
    {
        $this->itemId = $itemId;
        
        return $this;
    }
    
    public function getAdded(): int
    {
        return $this->added;
    }
    
    public function setAdded(int $added)
    {
        $this->added = $added;
        
        return $this;
    }
    
    public function getName(): string
    {
        return $this->name;
    }
    
    public function setName(string $name)
    {
        $this->name = $name;
        
        return $this;
    }

    public function getServer(): string
    {
        return $this->server;
    }

    public function setServer(string $server)
    {
        $this->server = $server;

        return $this;
    }

    public function isTriggerDataCenter(): bool
    {
        return $this->triggerDataCenter;
    }

    public function setTriggerDataCenter(bool $triggerDataCenter)
    {
        $this->triggerDataCenter = $triggerDataCenter;

        return $this;
    }

    public function getTriggerOption(): int
    {
        return $this->triggerOption;
    }
    
    public function getTriggerOptionFormula(): string
    {
        return str_ireplace(
            '[condition]', $this->triggerOption, self::TRIGGERS[$this->triggerOption]
        );
    }
    
    public function setTriggerOption(int $triggerOption)
    {
        $this->triggerOption = $triggerOption;
        
        return $this;
    }
    
    public function getTriggerValue(): string
    {
        return $this->triggerValue;
    }
    
    public function setTriggerValue(string $triggerValue)
    {
        $this->triggerValue = $triggerValue;
        
        return $this;
    }
    
    public function getTriggerLimit(): int
    {
        return $this->triggerLimit;
    }
    
    public function setTriggerLimit(int $triggerLimit)
    {
        $this->triggerLimit = $triggerLimit;
        
        return $this;
    }
    
    public function getTriggerDelay(): int
    {
        return $this->triggerDelay;
    }
    
    public function setTriggerDelay(int $triggerDelay)
    {
        $this->triggerDelay = $triggerDelay;
        
        return $this;
    }
    
    public function getTriggerLastSent(): int
    {
        return $this->triggerLastSent;
    }
    
    public function setTriggerLastSent(int $triggerLastSent)
    {
        $this->triggerLastSent = $triggerLastSent;
        
        return $this;
    }
    
    public function isTriggerHq(): bool
    {
        return $this->triggerHq;
    }
    
    public function setTriggerHq(bool $triggerHq)
    {
        $this->triggerHq = $triggerHq;
        
        return $this;
    }
    
    public function isTriggerNq(): bool
    {
        return $this->triggerNq;
    }
    
    public function setTriggerNq(bool $triggerNq)
    {
        $this->triggerNq = $triggerNq;
        
        return $this;
    }
    
    public function isTriggerActive(): bool
    {
        return $this->triggerActive;
    }
    
    public function setTriggerActive(bool $triggerActive)
    {
        $this->triggerActive = $triggerActive;
        
        return $this;
    }
    
    public function isNotifiedViaEmail(): bool
    {
        return $this->notifiedViaEmail;
    }
    
    public function setNotifiedViaEmail(bool $notifiedViaEmail)
    {
        $this->notifiedViaEmail = $notifiedViaEmail;
        
        return $this;
    }
    
    public function isNotifiedViaDiscord(): bool
    {
        return $this->notifiedViaDiscord;
    }
    
    public function setNotifiedViaDiscord(bool $notifiedViaDiscord)
    {
        $this->notifiedViaDiscord = $notifiedViaDiscord;
        
        return $this;
    }
}
