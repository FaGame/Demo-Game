﻿using UnityEngine;
using System.Collections;
using RTS;

public class MainMenu : Menu
{
		protected override void SetButtons ()
		{
				buttons = new string[]{"New Game","Change Player","Quit Game"};
		}

		protected override void HandleButton (string text)
		{
				switch (text) {
				case "New Game":
						NewGame ();
						break;
				case "Quit Game":
						ExitGame ();
						break;
				case "Change Player":
						ChangePlayer ();
						break;
				default:
						break;
				}
		}

		private void NewGame ()
		{
				ResourceManager.MenuOpen = false;
				Application.LoadLevel ("Map");
				Time.timeScale = 1.0f;
		}

		void OnLevelWasLoaded ()
		{
				Screen.showCursor = true;
				if (PlayerManager.GetPlayerName () == "") {
						//no player selected yet to SetPlayerMenu
						GetComponent<MainMenu> ().enabled = false;
						GetComponent<SelectPlayerMenu> ().enabled = true;
				} else {
						//player selected so enable MainMenu
						GetComponent<MainMenu> ().enabled = true;
						GetComponent<SelectPlayerMenu> ().enabled = false;
				}
		}

		private void ChangePlayer ()
		{
				GetComponent<MainMenu> ().enabled = false;
				GetComponent<SelectPlayerMenu> ().enabled = true;
				SelectionList.LoadEntries (PlayerManager.GetPlayerNames ());

		}
}