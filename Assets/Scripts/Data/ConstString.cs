public static class ConstString
{
    // Scene 이름
    public const string SCENE_MENU = "MenuScene";
    public const string SCENE_START = "GenerateScene";
    public const string SCENE_GAME = "GameScene";

    // Tag, Layer 이름
    public const string EXP_ORB = "ExperienceOrb";
    public const string TAG_ENEMY = "Enemy";
    public const string LAYER_ENEMY = "Enemy";
    public const string LAYER_PLAYER = "Player";
    public const string LAYER_TOWER = "MainTower";

    // Save Data 이름
    public const string GAME_RESULT_FILE = "GameResult.json";

    // ONNX Layer 이름 (netron)
    public const string GAN_LATENT_INPUT_NAME = "latent_vector"; // "onnx::Reshape_0";
    public const string GAN_LABEL_INPUT_NAME = "class_label";    // "labels";
    public const string GAN_OUTPUT_NAME = "generated_image";     // "71";

    // Volume 정보
    public const string PREF_VOLUME = "SavedVolume";
}