using UnityEngine;
using UnityEngine.Serialization;

namespace GUI {
	public class CharacterManager : MonoBehaviour {
		[SerializeField] private Sprite[] Character;
		[SerializeField] private Sprite[] Decorate;
		[SerializeField] private SpriteRenderer Face;
		[SerializeField] private SpriteRenderer Deco;
		[FormerlySerializedAs("chaIndex")] public byte faceIndex;
		public byte decIndex;

		// Start is called before the first frame update
		private void Start() {
			Face.sprite = Character[0];
			Deco.sprite = Decorate[0];
		}

		// Update is called once per frame
		private void Check() {
			faceIndex = faceIndex switch {
				255 => 8,
				> 8 => 0,
				_ => faceIndex
			};
		}

		private void DecCheck() {
			decIndex = decIndex switch {
				255 => 9,
				> 9 => 0,
				_ => decIndex
			};
		}

		public void DecNext() {
			decIndex += 1;
			DecCheck();
			Deco.sprite = Decorate[decIndex];
		}

		public void DecPrev() {
			decIndex -= 1;
			DecCheck();
			Deco.sprite = Decorate[decIndex];
		}

		public void Next() {
			faceIndex += 1;
			Check();
			Face.sprite = Character[faceIndex];
		}

		public void Prev() {
			faceIndex -= 1;
			Check();
			Face.sprite = Character[faceIndex];
		}
	}
}