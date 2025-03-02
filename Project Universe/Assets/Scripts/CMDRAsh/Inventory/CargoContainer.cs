using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Base;
using ProjectUniverse.Player;
using ProjectUniverse.Production.Resources;
using ProjectUniverse.UI;
using Unity.Netcode;
using ProjectUniverse.Player.PlayerController;

namespace ProjectUniverse.Items.Containers
{
    public class CargoContainer : MonoBehaviour
    {
        [SerializeField] private int maxWeight;
        [SerializeField] private int volume;
        private List<ItemStack> inventory = new List<ItemStack>();
        [SerializeField] private CargoUIController cargoui;
        //[SerializeField] private InventoryUIController invUI;
        [SerializeField] private Rigidbody cargoRbd;
        private float runningvolume = 0f;
        private float OrdMass;
        private bool isFull;

        public bool IsFull
        {
            get { return isFull; }
        }
        
        // Start is called before the first frame update
        void Start()
        {
            //cargoRbd = GetComponent<Rigidbody>();
            OrdMass = cargoRbd.mass;

            /*Consumable_Ingot ingot = new Consumable_Ingot("Ingot_Gold", 3, 10);
            ItemStack devIngotStack = new ItemStack("Ingot_Gold", 999, typeof(Consumable_Ingot));
            int i = 0;
            while (i < 3)
            {
                devIngotStack.AddItem(ingot);
                i += 1;
            }
            AddToInventory(devIngotStack);
            //AddToInventory(devIngotStack);
            cargoui.UpdateDisplay(inventory);
            UpdateRBMass();*/
        }

        public void UpdateRBMass()
        {
            float tempD = 0f;
            float tempV = 0f;
            //inventory mass is added to the rigidbody
            foreach (ItemStack item in inventory)
            {
                //Currently, there exist no items where count is not kg.
                //Ingots added in start are 5Kg added 3x to one stack.
                //components and other things will eventually need mass calc'ed.
                
                float density = 1.0f;
                float volume = 0f;
                //get item def
                if (item.GetOriginalType() == typeof(Consumable_Ingot))
                {
                    Consumable_Ingot ingot = (Consumable_Ingot)item.GetItemArray().GetValue(0);
                    density = ingot.GetIngotMass();
                    volume = ingot.IngotDef.GetDensity();
                    //IngotDefinition idef;
                    //IngotLibrary.IngotDictionary.TryGetValue(item.GetStackType(), out idef);
                    //density = idef.GetDensity();
                }
                else if(item.GetOriginalType() == typeof(Consumable_Ore))
                {
                    volume = 1f/1600f;//m^3/kg
                    density = 1f;
                }
                else
                {
                    density = 1f;
                    volume = 1 / 100f;
                }
                /*
                else if (item.GetOriginalType() == typeof(Consumable_Component))
                {
                    density = 2.0f;//eventually mass of the component will be the added masses of input materials.
                }
                else
                {
                    density = 1.0f;
                }
                */
                tempD += item.Size() * density;
                tempV += item.Size() * volume;
                
            }
            cargoRbd.mass = tempD;
            runningvolume = tempV;
            if (cargoRbd.mass > maxWeight || runningvolume > volume)
            {
                isFull = true;
            }
            else
            {
                isFull = false;
            }
        }

        public void ExternalInteractFunc()
        {
            //connect to player transfer ui
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                InventoryUIController invui = networkedClient.PlayerObject.gameObject.GetComponent<IPlayer_Inventory>().InventoryUI;
                networkedClient.PlayerObject.gameObject.GetComponent<SupplementalController>().FleetBoyOut = true;
                invui.gameObject.SetActive(true);
                invui.LockScreenAndFreeCursor();
                invui.SetCargoContainer(this);
                invui.SetContName("Storage Crate");
                invui.SetPlayerName("Player");
                invui.ReloadDisplay();
            }
        }

        public void DisplayInventory()
        {
            //invUI.SetCargoContainer(this);
            //invUI.SetContName(this.gameObject.name);
            //invUI.UpdateDisplay();
            //InventoriesSelector.transform.gameObject.SetActive(true);
            //InventoriesSelector.GetComponent<InventorySelectAndTransfer>().SetCargoContainer(this);
            //InventoriesSelector.GetComponent<InventorySelectAndTransfer>().SetPlayerInventory();
            //invUI.SetName(this.gameObject.name);
            //InventoriesSelector.GetComponent<InventorySelectAndTransfer>().SelectContainerInventory();
        }

        /// <summary>
        /// Find and remove any empty or null indexes in Index
        /// </summary>
        public void SanityCheck()
        {
            for (int i = 0; i < inventory.Count; i++)
            { 
                if(inventory[i] == null)
                {
                    inventory.RemoveAt(i);
                }
                else
                {
                    if(inventory[i].GetRealLength() <= 0)
                    {
                        inventory.RemoveAt(i);
                    }
                }
            }
        }

        //public InventoryUIController GetInventoryUI()
        //{
            //return invUI;
        //}

        public void InputFromPlayer(GameObject player)
        {
            IPlayer_Inventory playerInventory = player.GetComponent<IPlayer_Inventory>();
            inventory.Add(playerInventory.RemoveFromPlayerInventory(0));
            //cargoui.UpdateDisplay(inventory);
        }

        public int GetMaxWeight()
        {
            return maxWeight;
        }

        public int GetMaxVolume()
        {
            return volume;
        }

        public CargoUIController GetCargoUIController()
        {
            return cargoui;
        }

        public List<ItemStack> GetInventory()
        {
            return inventory;
        }

        public bool AddToInventory(ItemStack stack)
        {
            SanityCheck();
            if(cargoRbd.mass < maxWeight && runningvolume < volume)
            {
                for (int i = 0; i < inventory.Count; i++)
                {
                    if (inventory[i].CompareMetaData(stack))
                    {
                        //Debug.Log("Added to cont inventory");
                        ItemStack slaanesh = inventory[i].AddItemStack(stack);
                        if(slaanesh != null && slaanesh.GetRealLength() > 0)
                        {
                            inventory.Add(slaanesh);
                        }
                        return true;
                    }
                }
                //if the return is not hit, then there are no other compatible itemstacks
                inventory.Add(stack);
                UpdateRBMass();
                return true;
            }
            else 
            {
                isFull = true;
                return false;
            }
        }

        public bool RemoveFromInventory(ItemStack stack, out ItemStack returnstack)
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                //only run once, otherwise all things get cleared?
                Debug.Log(inventory[i].ToString() + " v " + stack.ToString());
                if (inventory[i] == stack)
                {
                    returnstack = inventory[i];
                    Debug.Log("RemoveAt: " + i);
                    inventory.RemoveAt(i);
                    UpdateRBMass();
                    return true;
                }
                /*
                if (inventory[i].CompareMetaData(stack))
                {
                    if(stack.Size() >= inventory[i].Size())
                    {
                        returnstack = inventory[i].RemoveItemData(stack.Size());
                        //returnstack = stack;
                        inventory.RemoveAt(i);
                    }
                    else
                    {
                        returnstack = inventory[i].RemoveItemData(stack.Size());
                        Debug.Log("Remaining:" + stack.Size());
                    }
                    return true;
                }*/
            }
            //if nothing above returned the value
            Debug.LogError("Attempted removal of non-existant item! \n" + "" + stack.ToString());
            returnstack = null;
            return false;
        }
        public ItemStack RemoveFromInventory<stacktype>(ItemStack removeFromStack, int atIndex)
        {
            int stackIndex = -1;
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] == removeFromStack)
                {
                    stackIndex = i;
                }
            }
            if (stackIndex != -1)
            {
                Debug.Log("Removing: " + inventory[stackIndex].GetItemArray().GetValue(atIndex));
                inventory[stackIndex].RemoveTArrayIndex<stacktype>(atIndex, out ItemStack stack);
                if (inventory[stackIndex].GetRealLength() <= 0f)
                {
                    inventory.RemoveAt(stackIndex);
                }
                UpdateRBMass();
                return stack;
            }
            else
            {
                return null;
            }
        }
    }
}