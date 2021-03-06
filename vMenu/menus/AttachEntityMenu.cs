﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient
{
    public class AttachEntityMenu
    {
        // Variables
        private Menu menu;

        public bool MiscRightAlignMenu { get; private set; } = UserDefaults.MiscRightAlignMenu;


        public int currentEntity;
        public int baseEntity;
        public int currEntity;
        public int entityOne;
        public int entityOneNewHeading = 0;
        public float entityOneNewX = 0;
        public float entityOneNewY = 0;
        public float entityOneNewZ = 0;
        public float lastX = 0;
        public float lastY = 0;
        public float lastZ = 0;
        public bool setupMode = false;
        public int currState = 0;
        public bool attached = false;

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            try
            {
                MenuController.MenuAlignment = MiscRightAlignMenu ? MenuController.MenuAlignmentOption.Right : MenuController.MenuAlignmentOption.Left;
            }
            catch (AspectRatioException)
            {
                Notify.Error(CommonErrors.RightAlignedNotSupported);
                // (re)set the default to left just in case so they don't get this error again in the future.
                MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Left;
                MiscRightAlignMenu = false;
                UserDefaults.MiscRightAlignMenu = false;
            }

            // Create the menu.
            menu = new Menu(Game.Player.Name, "Attach Entity Menu");
            // Create the menu items.

            MenuItem setupModeSelect = new MenuItem("Setup Mode", "Wipe Attacher and enter Setup Mode.");
            MenuItem baseEntitySelect = new MenuItem("Base Entity Select", "Select the base entity.");
            MenuItem entityOneSelect = new MenuItem("Target Entity Select", "Select the target entity.");
            MenuItem attachEntitySelect = new MenuItem("Attach Entity", "Attaches target entity to the base.");
            MenuItem detachEntitySelect = new MenuItem("Detach Entity", "Detaches target entity from the base.");


            menu.AddMenuItem(setupModeSelect);
            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == setupModeSelect)
                {
                    menu.ClearMenuItems();
                    menu.AddMenuItem(setupModeSelect);
                    currState = 0;
                    setupMode = true;
                    baseEntity = 0;
                    entityOne = 0;
                    menu.AddMenuItem(baseEntitySelect);
                } else if(item == baseEntitySelect)
                {
                    currState = 1;
                    baseEntity = currentEntity;                  
                    menu.RemoveMenuItem(baseEntitySelect);
                    menu.AddMenuItem(entityOneSelect);
                } else if(item == entityOneSelect)
                {
                    currState = 2;
                    entityOne = currentEntity;
                    menu.RemoveMenuItem(entityOneSelect);
                    AddEntityOneMenu();
                    menu.AddMenuItem(attachEntitySelect);
                    SetEntityCollision(baseEntity, false, true);
                } else if(item == attachEntitySelect)
                {
                    Vector3 targetPos = GetEntityCoords(entityOne, true);
                    Vector3 attachPos = GetOffsetFromEntityGivenWorldCoords(baseEntity, targetPos.X, targetPos.Y, targetPos.Z);
                    FreezeEntityPosition(entityOne, false);
                    SetEntityCollision(entityOne, true, true);
                    SetEntityCollision(baseEntity, true, true);
                    Vector3 rot = GetEntityRotation(entityOne,0);
                    Vector3 baseRot = GetEntityRotation(baseEntity, 0);
                    AttachEntityToEntity(entityOne, baseEntity, -1, attachPos.X, attachPos.Y, attachPos.Z, 0,0, rot.Z-baseRot.Z, false, true, true, false, 0, true);
                    setupMode = false;
                    currState = 3;
                    menu.ClearMenuItems();
                    menu.AddMenuItem(detachEntitySelect);
                } else if(item == detachEntitySelect)
                {
                    DetachEntity(entityOne, true, true);
                    FreezeEntityPosition(entityOne, false);
                    SetEntityCollision(entityOne, true, true);
                    menu.ClearMenuItems();
                    menu.AddMenuItem(setupModeSelect);
                    currState = 0;
                    setupMode = false;
                    baseEntity = 0;
                    entityOne = 0;
                    
                }
            };
        }

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }

        public void AddEntityOneMenu()
        {
            MenuSliderItem entityOneHeading = new MenuSliderItem("Entity Heading", 0, 360, 180);
            menu.AddMenuItem(entityOneHeading);

            MenuSliderItem entityOneX = new MenuSliderItem("Entity Left / Right", -200, 200, 0);
            menu.AddMenuItem(entityOneX);

            MenuSliderItem entityOneY = new MenuSliderItem("Entity Forward / Backward", -200, 200, 0);
            menu.AddMenuItem(entityOneY);

            MenuSliderItem entityOneZ = new MenuSliderItem("Entity Up / Down", -200, 200, 0);
            menu.AddMenuItem(entityOneZ);

            menu.OnSliderPositionChange += (sender, item, oldPosition, newPosition, index) =>
            {
                Vector3 currPos = GetEntityCoords(entityOne, true);
                if(item == entityOneHeading)
                {
                    entityOneNewHeading = newPosition;
                    SetEntityHeading(entityOne, newPosition);
                } else if(item == entityOneX)
                {
                    
                    entityOneNewX = ((newPosition - 100.0f) * 0.0005f);
                    
                    Vector3 newPos = GetOffsetFromEntityInWorldCoords(entityOne, lastX > entityOneNewX ? entityOneNewX : -entityOneNewX, 0.0f, 0.0f);
                    SetEntityCoordsNoOffset(entityOne, newPos.X, newPos.Y, newPos.Z, false, false, false);
                    lastX = entityOneNewX;
                } else if(item == entityOneY)
                {
                    entityOneNewY = ((newPosition - 100.0f) * 0.0005f);
                    Vector3 newPos = GetOffsetFromEntityInWorldCoords(entityOne, 0.0f,lastY > entityOneNewY ? entityOneNewY : -entityOneNewY, 0.0f);
                    SetEntityCoordsNoOffset(entityOne, newPos.X, newPos.Y, newPos.Z, false, false, false);
                    lastY = entityOneNewY;
                    Debug.WriteLine(entityOneNewY.ToString());
                } else if (item == entityOneZ)
                {
                    entityOneNewZ = ((newPosition - 100.0f) * 0.0005f);
                    Vector3 newPos = GetOffsetFromEntityInWorldCoords(entityOne, 0.0f, 0.0f,lastZ > entityOneNewZ ? entityOneNewZ : -entityOneNewZ);
                    SetEntityCoordsNoOffset(entityOne, newPos.X, newPos.Y, newPos.Z, false, false, false);
                    lastZ = entityOneNewZ;
                    Debug.WriteLine(entityOneNewZ.ToString());
                }

                FreezeEntityPosition(entityOne, true);
                SetEntityCollision(entityOne, false, true);
                if(IsEntityAttached(entityOne))
                {
                    DetachEntity(entityOne, true, true);
                    FreezeEntityPosition(entityOne, false);
                    SetEntityCollision(entityOne, true, true);

                }
            };
        }

    }
}
