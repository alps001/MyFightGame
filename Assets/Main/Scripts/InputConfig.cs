using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyFightGame
{
    public enum sex
    {
        Unknown,
        Male,
        Female
    }
    [System.Serializable]
    public class TestData
    {
        public float moveForwardSpeed = 4f; // How fast this character can move forward
        public float moveBackSpeed = 3.5f; // How fast this character can move backwards
    }

    public class InputConfig : ScriptableObject
    {
        public InputReferences[] inputReferences;

        public Texture2D profilePictureSmall;
        public string characterName;
        public sex gender;
        public Color alternativeColor;
        public AudioClip deathSound;
        public float height;
        public int age;
        public GameObject characterPrefab;

        public TestData[] moves = new TestData[0];

    }
}
