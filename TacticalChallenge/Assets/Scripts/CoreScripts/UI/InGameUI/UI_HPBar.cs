using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Michsky.MUIP;

public class UI_HPBar : MonoBehaviour
{
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private Image damageImage;
    [SerializeField] private Player targetPlayer;

    [SerializeField] private bool smooth = true;
    [SerializeField] private float smoothDuration = 0.12f;

    [Range(0f, 1f)][SerializeField] private float dangerThreshold = 0.33f;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color dangerColor;
    [SerializeField] private Color damageColor = new Color(1f, 0.85f, 0f, 0.9f);

    private Coroutine anim;
    private Coroutine damageAnim;
    private float prevValue;

    [SerializeField] private Image seperatorImage;
    [SerializeField] private float segmentHP = 100f; // 셰이더의 _Steps 값으로 사용

    private Material seperatorMaterial;

    private void Start()
    {
        if (targetPlayer == null)
        {
            targetPlayer = FindObjectOfType<Player>();
        }

        if (targetPlayer != null)
        {
            SetTarget(targetPlayer);
        }
    }

    public void SetTarget(Player p)
    {
        if (p == null) return;

        if (targetPlayer != null)
        {
            targetPlayer.OnHPChanged -= HandleHPChanged;
        }

        targetPlayer = p;

        if (seperatorImage != null)
        {
            seperatorMaterial = seperatorImage.material;
        }

        if (progressBar != null)
        {
            progressBar.maxValue = targetPlayer.MaxHP;
            progressBar.SetValue(targetPlayer.HP);

            UpdateShaderProperties();
        }

        if (damageImage != null)
        {
            float frac = SafeFraction(targetPlayer.HP, targetPlayer.MaxHP);
            damageImage.fillAmount = frac;
            damageImage.color = damageColor;
        }

        prevValue = targetPlayer.HP;

        targetPlayer.OnHPChanged += HandleHPChanged;
    }

    private void HandleHPChanged(float current, float max)
    {
        if (progressBar == null) return;

        float prev = prevValue;
        float now = current;
        prevValue = current;

        UpdateShaderProperties();

        if (!smooth)
        {
            progressBar.SetValue(now);
        }
        else
        {
            if (anim != null) StopCoroutine(anim);
            anim = StartCoroutine(AnimateMainBar(prev, now, smoothDuration));
        }

        float frac = SafeFraction(current, max);
        progressBar.loadingBar.color = (frac <= dangerThreshold) ? dangerColor : normalColor;

        if (damageImage != null)
        {
            float prevFrac = SafeFraction(prev, max);
            float nowFrac = frac;

            if (nowFrac < prevFrac)
            {
                damageImage.fillAmount = prevFrac;
                if (damageAnim != null) StopCoroutine(damageAnim);
                damageAnim = StartCoroutine(AnimateDamageOverlay(prevFrac, nowFrac, 0.6f));
            }
            else
            {
                if (damageAnim != null) StopCoroutine(damageAnim);
                damageImage.fillAmount = nowFrac;
            }
        }
    }

    private void UpdateShaderProperties()
    {
        if (seperatorMaterial == null || targetPlayer == null)
        {
            return;
        }

        float rectWidth = seperatorImage.rectTransform.rect.width;
        float maxHP = targetPlayer.MaxHP;
        float currentHP = targetPlayer.HP;

        seperatorMaterial.SetFloat("_Width", rectWidth);
        seperatorMaterial.SetFloat("_MaxHP", maxHP);
        seperatorMaterial.SetFloat("_HSRatio", SafeFraction(currentHP, maxHP));
        seperatorMaterial.SetFloat("_Steps", segmentHP); // 에디터에서 설정한 값을 셰이더에 전달
    }

    private IEnumerator AnimateMainBar(float fromValue, float toValue, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float v = Mathf.Lerp(fromValue, toValue, Mathf.Clamp01(t / duration));
            progressBar.SetValue(v);
            seperatorMaterial.SetFloat("_HSRatio", SafeFraction(v, targetPlayer.MaxHP));
            yield return null;
        }
        progressBar.SetValue(toValue);
        seperatorMaterial.SetFloat("_HSRatio", SafeFraction(toValue, targetPlayer.MaxHP));
        anim = null;
    }

    private IEnumerator AnimateDamageOverlay(float fromFrac, float toFrac, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float v = Mathf.Lerp(fromFrac, toFrac, Mathf.Clamp01(t / duration));
            damageImage.fillAmount = v;
            yield return null;
        }
        damageImage.fillAmount = toFrac;
        damageAnim = null;
    }

    private float SafeFraction(float cur, float max)
        => (max <= 0f) ? 0f : Mathf.Clamp01(cur / max);

    private void OnDestroy()
    {
        if (targetPlayer != null) targetPlayer.OnHPChanged -= HandleHPChanged;
    }
}