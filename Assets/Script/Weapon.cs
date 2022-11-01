using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickStart
{
    public class Weapon : MonoBehaviour
    {
        public float weaponSpead = 15.0f;
        public float weaponLife = 3.0f;
        public float weaponCooldown = 1.0f;
        public int weaponAmmo = 15;

        public GameObject weaponBullet;
        public Transform weaponFirePosition;
    }
}

