using UnityEngine;
using System.Collections;

public class BirdsManager : MonoBehaviour {

    public Bird []_birds;
	public GameplayCamera _camera;
	public Vector3 _initialPosition;

    private int _currentBirdIndex;

	// Use this for initialization
	void Start () {

        _birds[_currentBirdIndex].rigidbody2D.gravityScale = 0f;

        for(int i = 1; i < _birds.Length; i++)

            _birds[i].rigidbody2D.gravityScale = 1f;

	}

	// Update is called once per frame
	void Update () {

        if(!_birds[_currentBirdIndex] && !_birds[_currentBirdIndex].JumpToSlingshot)
        {
            _currentBirdIndex++;

            // If there are no more birds, reload the game
            if(_currentBirdIndex == _birds.Length)
            {
                Application.LoadLevel(Application.loadedLevel);
                return;
            }

            _birds[_currentBirdIndex].rigidbody2D.gravityScale = 0f;
            _birds[_currentBirdIndex].JumpToSlingshot = true;
        }

		// Move next bird to the slingshot
        if(_birds[_currentBirdIndex].JumpToSlingshot)
        {
            _birds[_currentBirdIndex].SetBirdOnSlingshot(transform.position + _initialPosition);

            if(_birds[_currentBirdIndex].transform.position == transform.position + _initialPosition)
            {
                _birds[_currentBirdIndex].JumpToSlingshot = false;
				_birds[_currentBirdIndex].OutOfSlingShot = false;
                _birds[_currentBirdIndex].rigidbody2D.velocity = Vector2.zero;
            }
        }

		// Kill current bird if it flies to outside the level
		if(_birds[_currentBirdIndex].OutOfSlingShot)

			if(_birds[_currentBirdIndex].transform.position.x > _camera.RightBound() + _camera.CalculateCameraRect().width/2f ||
			   _birds[_currentBirdIndex].transform.position.x < _camera.LeftBound()  - _camera.CalculateCameraRect().width/2f)
			{
				_birds[_currentBirdIndex].Die();
			}
	}

    public Bird GetCurrentBird()
    {
        if(_currentBirdIndex < _birds.Length)
            return _birds[_currentBirdIndex];

        return null;
    }
}
