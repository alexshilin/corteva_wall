using UnityEngine;
using System.Collections;
using System;

public class EaseCurve : MonoBehaviour
{
	public AnimationCurve linear = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
	public AnimationCurve curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
	public AnimationCurve easeIn;
	public AnimationCurve easeOut;
	public AnimationCurve easeInOut;
	public AnimationCurve easeOutBack;
	public AnimationCurve easeInOutBack;
	public AnimationCurve custom;
	public AnimationCurve custom2;

	public static EaseCurve Instance { get; private set; }

	void Awake()
	{
		Instance = this;
	}

	void Start(){

	}

	public void CamRect(Camera _target, Rect _start, Rect _end, float _duration, AnimationCurve _curve, Action _callback){
		StartCoroutine (EaseCamRect (_target, _start, _end, _duration, _curve, _callback));
	}

	private IEnumerator EaseCamRect(Camera _target, Rect _start, Rect _end, float _duration, AnimationCurve _curve, Action _callback){
		float t = 0.0f;
		float rate = 1 / _duration;
		while (t < 1) {
			t += rate * Time.deltaTime;
			_target.rect = new Rect(Mathf.Lerp (_start.x, _end.x, _curve.Evaluate (t)), 
								    Mathf.Lerp (_start.y, _end.y, _curve.Evaluate (t)), 
				  					Mathf.Lerp (_start.width, _end.width, _curve.Evaluate (t)), 
									Mathf.Lerp (_start.height, _end.height, _curve.Evaluate (t)));
			yield return null;
		}
		_target.rect = new Rect (_end.x, _end.y, _end.width, _end.height);
		//Debug.Log ("EaseCamRect finished "+_target.name+" in " + t + " sec. (" + _target.rect + " =?= " + _end);
		if(_callback!=null)
			_callback ();
	}




	public void Vec3(Transform _target, Vector3 _start, Vector3 _end, float _duration){
		StartCoroutine (EaseVec3 (_target, _start, _end, _duration, 0, linear, null, "world"));
	}
	public void Vec3(Transform _target, Vector3 _start, Vector3 _end, float _duration, float _delay){
		StartCoroutine (EaseVec3 (_target, _start, _end, _duration, _delay, linear, null, "world"));
	}
	public void Vec3(Transform _target, Vector3 _start, Vector3 _end, float _duration, float _delay, AnimationCurve _curve){
		StartCoroutine (EaseVec3 (_target, _start, _end, _duration, _delay, _curve, null, "world"));
	}
	public void Vec3(Transform _target, Vector3 _start, Vector3 _end, float _duration, float _delay, AnimationCurve _curve, Action _callback){
		StartCoroutine (EaseVec3 (_target, _start, _end, _duration, _delay, _curve, _callback, "world"));
	}
	public void Vec3(Transform _target, Vector3 _start, Vector3 _end, float _duration, float _delay, AnimationCurve _curve, Action _callback, string _space){
		StartCoroutine (EaseVec3 (_target, _start, _end, _duration, _delay, _curve, _callback, _space));
	}
	private IEnumerator EaseVec3(Transform _target, Vector3 _start, Vector3 _end, float _duration, float _delay, AnimationCurve _curve, Action _callback, string _space){
		float t = 0.0f;
		float rate = 1 / _duration;
		yield return new WaitForSeconds (_delay);
		while (t < 1) {
			if (_target != null) {
				t += rate * Time.deltaTime;
				if (_space == "local") {
					_target.localPosition = Vector3.Lerp (_start, _end, _curve.Evaluate (t));
				} else {
					_target.position = Vector3.Lerp (_start, _end, _curve.Evaluate (t));
				}
				yield return null;
			} else {
				Debug.LogWarning ("[EaseVec3] target object not found");
				yield break;
			}
		}
		if (_space == "local") {
			_target.localPosition = _end;
		} else {
			_target.position = _end;
		}
		//Debug.Log ("EaseVec3 finished "+_target.name+" in " + t + " sec. (" + _target.position + " =?= " + _end);
		if(_callback!=null)
			_callback ();
	}



	public void Rot(Transform _target, Quaternion _start, float _rotDegrees, Vector3 _axis, float _duration, float _delay, AnimationCurve _curve){
		StartCoroutine (EaseRot (_target, _start, _rotDegrees, _axis, _duration, _delay, _curve, null));
	}
	public void Rot(Transform _target, Quaternion _start, float _rotDegrees, Vector3 _axis, float _duration, float _delay, AnimationCurve _curve, Action _callback){
		StartCoroutine (EaseRot (_target, _start, _rotDegrees, _axis, _duration, _delay, _curve, _callback));
	}
	private IEnumerator EaseRot(Transform _target, Quaternion _start, float _rotDegrees, Vector3 _axis, float _duration, float _delay, AnimationCurve _curve, Action _callback){
		float t = 0.0f;
		float rate = 1 / _duration;
		yield return new WaitForSeconds (_delay);
		while (t < 1) {
			t += rate * Time.deltaTime;
			_target.localRotation = _start * Quaternion.AngleAxis(_curve.Evaluate (t) * _rotDegrees, _axis);
			yield return null;
		}
		_target.localRotation = _start * Quaternion.AngleAxis(_rotDegrees, _axis);
		//Debug.Log ("EaseVec3 finished "+_target.name+" in " + t + " sec. (" + _target.position + " =?= " + _end);
		if(_callback!=null)
			_callback ();
	}



	public void Scl(Transform _target, Vector3 _start, Vector3 _end, float _duration, float _delay, AnimationCurve _curve){
		StartCoroutine (EaseScl (_target, _start, _end, _duration, _delay, _curve, null));
	}
	public void Scl(Transform _target, Vector3 _start, Vector3 _end, float _duration, float _delay, AnimationCurve _curve, Action _callback){
		StartCoroutine (EaseScl (_target, _start, _end, _duration, _delay, _curve, _callback));
	}
	private IEnumerator EaseScl(Transform _target, Vector3 _start, Vector3 _end, float _duration, float _delay, AnimationCurve _curve, Action _callback){
		float t = 0.0f;
		float rate = 1 / _duration;
		yield return new WaitForSeconds (_delay);
		while (t < 1) {
			t += rate * Time.deltaTime;
			_target.localScale = Vector3.Lerp (_start, _end, _curve.Evaluate (t));
			yield return null;
		}
		_target.localScale = _end;
		//Debug.Log ("EaseVec3 finished "+_target.name+" in " + t + " sec. (" + _target.position + " =?= " + _end);
		if(_callback!=null)
			_callback ();
	}



	public void MatColor(Material _mat, Color32 _startColor, Color32 _endColor, float _duration, float _delay, AnimationCurve _curve){
		StartCoroutine (MatColor (_mat, _startColor, _endColor, _duration, _delay, _curve, null));
	}
	IEnumerator MatColor(Material _mat, Color32 _startColor, Color32 _endColor, float _duration, float _delay, AnimationCurve _curve, Action _callback)
	{
		float t = 0.0f;
		float rate = 1 / _duration;
		yield return new WaitForSeconds (_delay);
		while (t < 1) {
			t += rate * Time.deltaTime;
			Color32 currentColor = Color32.Lerp(_startColor, _endColor, _curve.Evaluate (t));
			_mat.color = currentColor;
			yield return null;
		}
		if(_callback!=null)
			_callback ();
	}
}