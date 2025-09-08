using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ACF.Tests
{
	// [ExecuteAlways]
	[RequireComponent(typeof(Image))]
	public class HealthBarTest : MonoBehaviour
	{
		private const string STEP = "_Steps";
		private const string RATIO = "_HSRatio";
		private const string WIDTH = "_Width";
		private const string THICKNESS = "_Thickness";
		
		private static readonly int floatSteps = Shader.PropertyToID(STEP);
		private static readonly int floatRatio = Shader.PropertyToID(RATIO);
		private static readonly int floatWidth = Shader.PropertyToID(WIDTH);
		private static readonly int floatThickness = Shader.PropertyToID(THICKNESS);
		
		[Range(0, 2800f)] public float Hp = 1000f;
		[Range(0, 2800f)] public float MaxHp = 1000f;
		[Range(0, 920f)] public float Sp = 0f;
		[Range(0, 10f)] public float speed = 3f;
		
		public float hpShieldRatio;
		public float RectWidth = 100f;
		[Range(0, 5f)]public float Thickness = 2f;
		
		public Image hp;
		public Image damaged;
		public Image sp;
		public Image separator;

		[ContextMenu("Create Material")]
		private void CreateMaterial()
		{
			// if (separator.material == null)
			{
				separator.material = new Material(Shader.Find("ABS/UI/Health Separator"));
			}
		}

		private IEnumerator Start()
		{
			yield return new WaitForSeconds(2.0f);

			Hp = 1500;
			MaxHp = 1500;
			Sp = 400;

			while (Sp > 0)
			{
				Sp -= 280 * Time.deltaTime;
				yield return null;
			}

			Sp = 0;

			yield return new WaitForSeconds(2f);

			for (int i = 0; i < 8; i++)
			{
				Hp -= 120;
				yield return new WaitForSeconds(1f);
			}
			
			for (int i = 0; i < 8; i++)
			{
				MaxHp += 200;
				Hp = MaxHp;
				
				yield return new WaitForSeconds(1f);
			}
			
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif
		}
		

		private void Update()
		{
			if (MaxHp < Hp)
			{
				MaxHp = Hp;
			}

			float step;

			// 쉴드가 존재 할 때
			if (Sp > 0)
			{
				// 현재체력 + 쉴드 > 최대 체력
				if (Hp + Sp > MaxHp)
				{
					hpShieldRatio = Hp / (Hp + Sp);
					sp.fillAmount = 1f;
					step = (Hp) / 300f;
					hp.fillAmount = Hp / (Hp + Sp);
				}
				else
				{
					sp.fillAmount = (Hp + Sp) / MaxHp;
					hpShieldRatio = Hp / MaxHp;
					step = Hp / 300f;
				
					hp.fillAmount = Hp / MaxHp;
				}
			}
			else
			{
				sp.fillAmount = 0f;
				step = MaxHp / 300f;
				hpShieldRatio = 1f;
				
				hp.fillAmount = Hp / MaxHp;
			}
			
			// sp.fillAmount = 1 - hpShieldRatio;
			
			damaged.fillAmount = Mathf.Lerp(damaged.fillAmount, hp.fillAmount, Time.deltaTime * speed);
			
			separator.material.SetFloat(floatSteps, step);
			separator.material.SetFloat(floatRatio, hpShieldRatio);
			separator.material.SetFloat(floatWidth, RectWidth);
			separator.material.SetFloat(floatThickness, Thickness);
		}
	}
}