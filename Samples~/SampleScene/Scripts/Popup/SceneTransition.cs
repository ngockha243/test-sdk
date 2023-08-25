using System.Collections;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

using Extensions;
using Utilities;
using Parameters;
using Services;
using UnityEngine.UI;
using Spine;

public class SceneTransition : MonoBehaviour
{
	[SerializeField] private Animator animator;
	[SerializeField] private float transitionTime;

	private GameServices gameServices;
	private PlayerService playerService;
	private System.Action onOpen;
	private void Awake()
	{
		animator.ThrowIfNull();

		//GameObject go = GameObject.FindGameObjectWithTag(Constanst.ServicesTag);
		//if (go != null)
		//{
		//	gameServices = go.GetComponent<GameServices>();
		//	playerService = gameServices.GetService<PlayerService>();
		//}
		//else
		//{
		//	SceneManager.LoadSceneAsync(Constanst.EntryScene);
		//	return;
		//}

		GameObject go = GameObject.FindGameObjectWithTag(Constants.ParamsTag);
		if (go != null)
		{
			PopupParameter popUpParameter = go.GetComponent<PopupParameter>();
			onOpen = popUpParameter.GetAction(ActionType.TransitionOption);
		}

		TransitionScene(onOpen);
	}
	public void TransitionScene(System.Action onAction)
	{
		StartCoroutine(WaitTransition(onAction));
	}
	private IEnumerator WaitTransition(System.Action onAction)
	{
		yield return new WaitForSeconds(transitionTime);
		onAction?.Invoke();
	}
	public void OnTransitionEnd()
	{
		PopupHelpers.Close(gameObject.scene);
	}
}
