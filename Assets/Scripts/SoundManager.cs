using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("BGM")]
    [SerializeField] private AudioClip menuBGM;
    [SerializeField] private AudioClip[] gameBGMs;

    [Header("효과음")]
    [SerializeField] private AudioClip shootSound;      // projectile 발사
    [SerializeField] private AudioClip colSound;        // enemy와 충돌
    [SerializeField] private AudioClip hitSound;        // projectile 타격
    [SerializeField] private AudioClip enemyDieSound;   // 적 사망
    [SerializeField] private AudioClip xpSound;         // 경험치 획득
    [SerializeField] private AudioClip levelUpSound;    // 레벨업
    [SerializeField] private AudioClip gameOverSound;   // 게임오버
    [SerializeField] private AudioClip stageClearSound; // 스테이지 클리어

    [Header("Skill")]
    [SerializeField] private AudioClip birdSkillSound;
    [SerializeField] private AudioClip dogSkillSound;
    [SerializeField] private AudioClip fishSkillSound;

    private const string VOLUME = "SavedVolume";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 유지
            SetVolume(PlayerPrefs.GetFloat(VOLUME, 0.5f)); // 이전 볼륨 설정 값 유지, 없으면 기본값(0.5)
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬 로딩이 끝나면 자동으로 호출
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Menu") // 캐릭터 생성 (Menu ~ Select)
        {
            PlayBGM(menuBGM);
        }
        else if (scene.name == "GameScene")
        {
            int randomBGM = Random.Range(0, gameBGMs.Length);
            PlayBGM(gameBGMs[randomBGM]);
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        // 중복 방지
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // SFX는 중복 재생 가능
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.pitch = Random.Range(0.9f, 1.1f); // 연속 발사 타격감
        sfxSource.PlayOneShot(clip);
    }

    // 볼륨 조절
    public void SetVolume(float volume)
    {
        // 0.0 ~ 1.0, default 0.5
        bgmSource.volume = volume;
        sfxSource.volume = volume;
        PlayerPrefs.SetFloat(VOLUME, volume);
    }

    // 호출 시 변수 없이 함수만
    public void PlayShoot() => PlaySFX(shootSound);
    public void PlayPlayerHit() => PlaySFX(colSound);
    public void PlayEnemyHit() => PlaySFX(hitSound);
    public void PlayEnemyDie() => PlaySFX(enemyDieSound);
    public void PlayGetExp() => PlaySFX(xpSound);
    public void PlayLevelUp() => PlaySFX(levelUpSound);
    public void PlayGameOver() => PlaySFX(gameOverSound);
    public void PlayStageClear() => PlaySFX(stageClearSound);
    public void PlayBirdSkill() => PlaySFX(birdSkillSound);
    public void PlayDogSkill() => PlaySFX(dogSkillSound);
    public void PlayFishSkill() => PlaySFX(fishSkillSound);

    public float GetVolume()
    {
        return bgmSource.volume;
    }
}