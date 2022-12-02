using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

namespace QuickStart
{
    public class PlayerScript : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnNameChanged))]
        public string playerName;

        [SyncVar(hook = nameof(OnColorChanged))]
        public Color playerColor = Color.white;
        
        public TextMeshPro playerNameText;
        public GameObject floatingInfo;

        private Material playerMaterialClone;
        private SceneScript sceneScript;

        private Weapon activeWeapon;
        private float weaponCooldownTime;
        
        private void Awake()
        {
            // allow all players to run this
            sceneScript = GameObject.Find("SceneReference").GetComponent<SceneReference>().sceneScript;

            // disable all weapons
            foreach (var item in weaponArray)
                if (item != null)
                    item.SetActive(false);

            if (selectedWeaponLocal < weaponArray.Length && weaponArray[selectedWeaponLocal] != null)
            {
                activeWeapon = weaponArray[selectedWeaponLocal].GetComponent<Weapon>();
                sceneScript.UIAmmo(activeWeapon.weaponAmmo);
            }
        }

        [Command]
        public void CmdSendPlayerMessage()
        {
            if(sceneScript)
            {
                sceneScript.statusText = $"{playerName} says hello {Random.Range(10, 99)}";
            }
        }

        public override void OnStartLocalPlayer()
        {
            sceneScript.playerScript = this;
            Camera.main.transform.SetParent(transform);
            Camera.main.transform.localPosition = new Vector3(0, 0, 0);

            floatingInfo.transform.localPosition = new Vector3(0, 1.3f, 0.3f);
            floatingInfo.transform.localScale = new Vector3(-1f, 1f, 1f);

            string name = "Player" + Random.Range(100, 999);
            Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
            CmdSetupPlayer(name, color);
        }

        [Command]
        public void CmdSetupPlayer(string _name, Color _col)
        {
            // player info sent to server, then server updates sync vars which handles it on all clients
            playerName = _name;
            playerColor = _col;
            sceneScript.statusText = $"{playerName} joined.";
        }

        private int selectedWeaponLocal = 1;
        public GameObject[] weaponArray;

        [SyncVar(hook = nameof(OnWeaponChanged))]
        public int activeWeaponSynced = 1;

        void OnWeaponChanged(int oldWeapon, int newWeapon)
        {
            // disable old weapon
            // in range and not null
            if (0 < oldWeapon && oldWeapon < weaponArray.Length && weaponArray[oldWeapon] != null)
                weaponArray[oldWeapon].SetActive(false);

            // enable new weapon
            // in range and not null
            if (0 < newWeapon && newWeapon < weaponArray.Length && weaponArray[newWeapon] != null)
            {
                weaponArray[newWeapon].SetActive(true);
                // 그 무기에 맞는 총알로 변경해줌
                activeWeapon = weaponArray[activeWeaponSynced].GetComponent<Weapon>();
                if (isLocalPlayer)
                    sceneScript.UIAmmo(activeWeapon.weaponAmmo);
            }
        }

        [Command]
        public void CmdChangeActiveWeapon(int newIndex)
        {
            activeWeaponSynced = newIndex;
        }

        void Update()
        {
            if (!isLocalPlayer)
            {
                // make non-local players run this
                floatingInfo.transform.LookAt(Camera.main.transform);
                return;
            }

            float moveX = Input.GetAxis("Horizontal") * Time.deltaTime * 110.0f;
            float moveZ = Input.GetAxis("Vertical") * Time.deltaTime * 4f;

            transform.Rotate(0, moveX, 0);
            transform.Translate(0, 0, moveZ);

            if (Input.GetButtonDown("Jump")) //Jump is space
            {
                Debug.Log("switch weapon!! ");
                selectedWeaponLocal += 1;

                if (selectedWeaponLocal > weaponArray.Length)
                    selectedWeaponLocal = 1;

                CmdChangeActiveWeapon(selectedWeaponLocal);
            }

            if (Input.GetButtonDown("Fire1")) // "Fire1 is left mouse click
            {
                if (activeWeapon && Time.time > weaponCooldownTime && activeWeapon.weaponAmmo > 0)
                {
                    weaponCooldownTime = Time.time + activeWeapon.weaponCooldown;
                    activeWeapon.weaponAmmo -= 1;
                    sceneScript.UIAmmo(activeWeapon.weaponAmmo);
                    CmdShootRay();
                }
            }
        }
        
        [Command]
        void CmdShootRay()
        {
            RpcFireWeapon();
        }

        [ClientRpc]
        void RpcFireWeapon()
        {
            //bulletAudio.Play(); 
            GameObject bullet = Instantiate(activeWeapon.weaponBullet, activeWeapon.weaponFirePosition.position, activeWeapon.weaponFirePosition.rotation);
            bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * activeWeapon.weaponSpead;
            Destroy(bullet, activeWeapon.weaponLife);
        }

        void OnNameChanged(string _Old, string _New)
        {
            playerNameText.text = playerName;
        }

        void OnColorChanged(Color _Old, Color _New)
        {
            playerNameText.color = _New;
            playerMaterialClone = new Material(GetComponent<Renderer>().material);
            playerMaterialClone.color = _New;
            GetComponent<Renderer>().material = playerMaterialClone;
        }
    }
}

