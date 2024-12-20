public class Define
{
    public enum WeaponType
    {
        None,
        Melee,
        Range,
        Target
    }

    public enum PlayerType
    {
        Warrior = 0,
        Archer = 1,
        Wizard = 2
    }

    public enum MonsterType
    {
        RabbyYoungGreen,
        RabbyGreen,
        RabbyQueenGreen,
        RabbyYoungBrown,
        RabbyBrown,
        RabbyQueenBrown,
        RabbyYoungWhite,
        RabbyWhite,
        RabbyQueenWhite,
        DevilTree,
        DevilTreeCandy,
        DevilTreeNightmare,
        DevilTreeWither,
        Turnipa_Bitter,
        Turnipa_Sour,
        Turnipa_Sweet,
        Beez_Darkness,
        Beez_Honey,
        Beez_Strawbery,
        Planta_Kid,
        Planta_Geezer,
        Planta_Queen,
        Planta_Shadow,
        Planta_Slave,
        Flower_Dryad_BOSS,
        Max
    }

    public enum ObjectType
    {
        Unknown,
        Player,
        Monster,
    }

    public enum State
    {
        None,
        Die,
        Moving,
        Idle,
        Jumping,
        Dash,
        Run,
        Chase,
        Return,
        Patrol,
        Hit,
        Stun,
        Dead,
        OnWall,
        NormalAttack,
        Skill,
        Skill2,
        Skill3,
        Skill4,
        Skill5,
        MAX
    }

    public enum Layer
    {
        Monster = 6,
        Ground = 7,
        Block = 8,
        Player = 9,
        Wall = 10,
    }
    public enum Scene
    {
        Unknown,
        Offline,
        Online,
        Lobby,
        Town,
        InstanceScene,
        MAX
    }

    public enum InstanceScene
    {
        Unknown,
        ParadiseInTheForest
    }

    public enum Sound2D
    {
        Bgm,
        Effect2D,
        MaxCount,
    }

    public enum Sound3D
    {
        Effect3D,
    }

    public enum UIEvent
    {
        Click,
        Enter,
        Down,
        Drag,
        Up,
        Exit
    }

    public enum MouseEvent
    {
        Press,
        PointerDown,
        PointerUp,
        Click,
    }

    public enum CameraMode
    {
        QuaterView,
    }

    public enum  CameraTargetOffset
    {
        Bottom,
        HalfBottom,
        Middle,
        HalfTop,
        Top
    }

    public enum PlayerInputAction
    {
        Move,
        Look,
        Jump,
        Dash,
        Run,
        NormalAttack,
        PickUp,
        Esc,
        Enter,
        Fire,
        I,
        P,
        M,
        U
    }

    public enum ItemType
    {
        None = 0,
        Countable = 1,
        Equip = 2,
        Coin = 3,
        ETC = 4
    }

    public enum ItemSubType
    {
        None = 0,
        MeleeWeapon = 1,
        RangeWeapon = 2,
        TargetWeapon = 3,
        Helmet = 4,
        Shoulder = 5,
        Top = 6,
        Pants = 7,
        Belt = 8,
        Shoes = 9,
        Ring = 10,
        Necklace = 11,
        Bracelet = 12,
        Consumption = 13,
        ETC = 14
    }

    public enum ItemGrade
    {
        Common = 0,
        Normal = 1,
        Rare = 2,
        Unique = 3,
        Legend = 4,
        Epic = 5,
    }

    public enum EffectType
    {
        None = 0,
        HP = 1,
        MaxHP = 2,
        MP = 3,
        MaxMP = 4,
        Speed = 5,
        Jump = 6
    }

    public enum InstanceBackground
    {
        Winter_Background,
        Lava_Background,
        Forest_Background,
        Desert_Background,
        Cave_Background
    }
}


