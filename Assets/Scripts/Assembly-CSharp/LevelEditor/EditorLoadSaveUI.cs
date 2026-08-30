using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LevelEditor
{
	public class EditorLoadSaveUI : EditorUIBase
	{
		[SerializeField]
		private LevelCreator m_LevelCreator;

		[SerializeField]
		private Button m_SaveButton;

		[SerializeField]
		private Button m_LoadButton;

		[SerializeField]
		private Button m_UploadButton;

		[SerializeField]
		private Button m_ClearButton;

		[SerializeField]
		private Button m_AutoSaveButton;

		[SerializeField]
		private TextMeshProUGUI m_CurrentLoadedLevelText;

		[SerializeField]
		private TMP_InputField m_SaveNameInputField;

		[SerializeField]
		private CodeStateAnimation m_LoadGridAnimation;

		[SerializeField]
		private Transform m_LoadGrid;

		[SerializeField]
		private GameObject m_LevelPrefab;

		[SerializeField]
		private GameObject m_ScreenShotCameraObject;

		private static EditorLoadSave m_LoadSave;

		private static LevelManager m_LevelManager;

		private int m_AutoSaveTimer;

		public List<int> AutosaveTimers = new List<int>();

		private int myCurrAutosaveIndex;

		private float m_TimeSinceAutoSave;

		public GameObject ScreenshotCameraObject
		{
			get
			{
				return m_ScreenShotCameraObject;
			}
		}

		private void Update()
		{
			if (m_AutoSaveTimer > 0)
			{
				m_TimeSinceAutoSave += Time.deltaTime;
				if (m_TimeSinceAutoSave / 60f > (float)m_AutoSaveTimer)
				{
					m_TimeSinceAutoSave = 0f;
					DoAutosave();
				}
			}
			else if (m_TimeSinceAutoSave > 0f)
			{
				m_TimeSinceAutoSave = 0f;
			}
		}

		private void DoAutosave()
		{
			Debug.Log("Doing Autosave");
			OnSavedClicked(true);
		}

		private void Start()
		{
			m_LoadSave = EditorLoadSave.Instance;
			m_LevelManager = LevelManager.Instance;
			InitListeners();
			if (AutosaveTimers.Count == 0)
			{
				AutosaveTimers.Add(0);
			}
		}

		private void InitListeners()
		{
			m_AutoSaveButton.onClick.AddListener(() =>
			{
				Validate(OnAutosaveClicked, WindowOpen.Autosave);
			});
			m_SaveButton.onClick.AddListener(() =>
			{
				Validate(OnSavedClicked, WindowOpen.Save);
			});
			m_LoadButton.onClick.AddListener(() =>
			{
				Validate(OnLoadClicked, WindowOpen.Load);
			});
			m_UploadButton.onClick.AddListener(() =>
			{
				Validate(OnUploadClicked, WindowOpen.Upload);
			});
			m_ClearButton.onClick.AddListener(() =>
			{
				Validate(OnClearClicked, WindowOpen.Clear);
			});
			AddNewButton(m_SaveButton, WindowOpen.Save);
			AddNewButton(m_LoadButton, WindowOpen.Load);
			AddNewButton(m_UploadButton, WindowOpen.Upload);
			AddNewButton(m_ClearButton, WindowOpen.Clear);
			m_SaveNameInputField.onSelect.AddListener(OnSaveInputFieldSelect);
			m_SaveNameInputField.onDeselect.AddListener(OnSaveInputFieldDeSelect);
			m_LevelManager.AddOnObjectAddedAction(() =>
			{
				m_LoadSave.SetHasBeenTouched(true);
			});
		}

		public void OnAutosaveClicked()
		{
			myCurrAutosaveIndex++;
			if (myCurrAutosaveIndex >= AutosaveTimers.Count)
			{
				myCurrAutosaveIndex = 0;
			}
			m_TimeSinceAutoSave = 0f;
			m_AutoSaveTimer = AutosaveTimers[myCurrAutosaveIndex];
			TextMeshProUGUI componentInChildren = m_AutoSaveButton.GetComponentInChildren<TextMeshProUGUI>();
			componentInChildren.text = "AUTOSAVE : ";
			if (m_AutoSaveTimer == 0)
			{
				componentInChildren.text += "OFF";
				return;
			}
			componentInChildren.text += m_AutoSaveTimer;
			componentInChildren.text += "MIN";
		}

		public void OnClearClicked()
		{
			DialougePanelUI.Instance.GiveChoice("Do You Want To Clear Your Current Level?", () =>
			{
				m_LevelManager.ClearLevel(true);
				ClearCurrentLoadedMap();
			}, () =>
			{
			});
		}

		private void OnSaveInputFieldSelect(string text)
		{
			Debug.Log("SelectInputField " + text);
			LevelEditorInputManager.SetNewKeyboardInputState(false);
		}

		private void OnSaveInputFieldDeSelect(string text)
		{
			Debug.Log("Deselct Inputfield " + text);
			LevelEditorInputManager.SetNewKeyboardInputState(true);
		}

		private void OnSavedClicked()
		{
			OnSavedClicked(false);
		}

		private void OnSavedClicked(bool alwaysOverwrite = false)
		{
			Debug.Log("Clicked Save Level!");
			MapSizeHandler.Instance.mapSizeFrame.root.gameObject.SetActive(false);
			AspectFix component = ScreenshotHandler.Instance.GetCamera().GetComponent<AspectFix>();
			component.scale = true;
			component.UpdateSize();
			bool gridVisibility = m_LevelCreator.GetGridVisibility();
			m_LevelCreator.ShowGrid(false);
			string newSaveName = m_SaveNameInputField.text.Trim();
			SaveNameResultEnum saveNameResultEnum = ValidateLevelName(newSaveName);
			if (saveNameResultEnum == SaveNameResultEnum.Ok)
			{
				Action a = () =>
				{
					UpdateCurrentLoadedMap();
				};
				if (EditorLoadSave.CheckIfOverwriteLevel(newSaveName))
				{
					Action action = () =>
					{
						m_LoadSave.SaveLevel(newSaveName, a);
						m_LoadSave.SetHasBeenTouched(false);
						DialougePanelUI.Instance.Prompt("Save Sucessful!");
					};
					if (alwaysOverwrite)
					{
						action();
					}
					else
					{
						Action noAction = () =>
						{
							Debug.Log("Dont Overwrite!");
						};
						DialougePanelUI.Instance.GiveChoice("Do You Want Do Overwrite Level With Name: " + newSaveName + " ? ", action, noAction);
					}
				}
				else
				{
					m_LoadSave.SaveLevel(newSaveName, a);
					m_LoadSave.SetHasBeenTouched(false);
					DialougePanelUI.Instance.Prompt("Save Sucessful!");
				}
			}
			else
			{
				Debug.Log("SaveName Error: " + saveNameResultEnum);
				DialougePanelUI.Instance.Prompt("Save Error: " + GetErrorMessage(saveNameResultEnum));
			}
			MapSizeHandler.Instance.mapSizeFrame.root.gameObject.SetActive(true);
			m_LevelCreator.ShowGrid(gridVisibility);
			component.scale = false;
		}

		private void UpdateCurrentLoadedMap()
		{
			m_SaveNameInputField.text = m_LoadSave.CurrentLoadedMap;
			m_CurrentLoadedLevelText.text = m_LoadSave.CurrentLoadedMap;
		}

		private void ClearCurrentLoadedMap()
		{
			m_SaveNameInputField.text = string.Empty;
			m_CurrentLoadedLevelText.text = "New Map";
		}

		private SaveNameResultEnum ValidateLevelName(string levelName)
		{
			if (string.IsNullOrEmpty(levelName))
			{
				return SaveNameResultEnum.Empty;
			}
			char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
			if (invalidFileNameChars.Any((char c) => levelName.Contains(c)))
			{
				return SaveNameResultEnum.InvalidCharacters;
			}
			return SaveNameResultEnum.Ok;
		}

		private void OnLoadClicked()
		{
			Debug.Log("Clicked Load Level!");
			bool state = m_LoadGridAnimation.state1;
			if (state)
			{
				if (PopulateGrid())
				{
					m_LoadGridAnimation.state1 = !state;
				}
				LevelEditorInputManager.SetNewInputState(false, false);
			}
			else
			{
				if (ClearGrid())
				{
					m_LoadGridAnimation.state1 = !state;
				}
				LevelEditorInputManager.SetNewInputState(true, true);
			}
		}

		private bool ClearGrid()
		{
			int childCount = m_LoadGrid.childCount;
			for (int i = 0; i < childCount; i++)
			{
				UnityEngine.Object.Destroy(m_LoadGrid.GetChild(i).gameObject);
			}
			return true;
		}

		private bool PopulateGrid()
		{
			ClearGrid();
			DirectoryInfo[] array = WorkshopMapsLoader.Instance.LoadAllLocalMaps();
			WorkshopMapWrapper[] array2 = WorkshopMapsLoader.Instance.LoadAllWorkshopMaps();
			DirectoryInfo[] array3 = array;
			foreach (DirectoryInfo item in array3)
			{
				string levelName = item.Name;
				bool isWorkshop = false;
				Action onClickAction = () =>
				{
					Action action = () =>
					{
						m_LoadGridAnimation.state1 = true;
						m_LoadSave.LoadLevel(levelName, isWorkshop, string.Empty);
						LevelManager.Instance.PopulateLevel();
						m_LoadSave.SetHasBeenTouched(false);
						UpdateCurrentLoadedMap();
						LevelCreator.Instance.GenerateSnapCornersFaces();
						LevelEditorInputManager.SetNewInputState(true, true);
					};
					if (m_LoadSave.HasbeenTouched)
					{
						DialougePanelUI.Instance.GiveChoice("Loading This Map Will Overwrite Any Unsaved Progress, Continue?", action, () =>
						{
						});
					}
					else
					{
						action();
					}
				};
				Action onDeleteAction = () =>
				{
					DialougePanelUI.Instance.GiveChoice("Do You Want To Delete Map: " + levelName + " ?", () =>
					{
						m_LoadSave.DeleteLevel(item.Name);
						PopulateGrid();
					}, () =>
					{
					});
				};
				GameObject gameObject = UnityEngine.Object.Instantiate(m_LevelPrefab, m_LoadGrid);
				gameObject.GetComponent<LevelButtonUI>().Init(levelName, isWorkshop, true, onClickAction, onDeleteAction);
				gameObject.SetActive(true);
			}
			WorkshopMapWrapper[] array4 = array2;
			foreach (WorkshopMapWrapper item2 in array4)
			{
				string levelName2 = item2.LevelName;
				string publishID = item2.PublishID.ToString();
				bool isWorkshop2 = true;
				Action onClickAction2 = () =>
				{
					Action action = () =>
					{
						m_LoadGridAnimation.state1 = true;
						m_LoadSave.LoadLevel(levelName2, isWorkshop2, publishID);
						LevelManager.Instance.PopulateLevel();
						m_LoadSave.SetHasBeenTouched(false);
						UpdateCurrentLoadedMap();
						LevelEditorInputManager.SetNewInputState(true, true);
					};
					if (m_LoadSave.HasbeenTouched)
					{
						DialougePanelUI.Instance.GiveChoice("Loading This Map Will Overwrite Any Unsaved Progress, Continue?", action, () =>
						{
						});
					}
					else
					{
						action();
					}
				};
				Action onDeleteAction2 = () =>
				{
					DialougePanelUI.Instance.GiveChoice("Do You Want To Unsubscribe From Map: " + levelName2 + " ?", () =>
					{
						WorkshopMapsLoader.Instance.DeleteWorkshopMap(item2, () =>
						{
							PopulateGrid();
						});
					}, () =>
					{
					});
				};
				GameObject gameObject2 = UnityEngine.Object.Instantiate(m_LevelPrefab, m_LoadGrid);
				gameObject2.GetComponent<LevelButtonUI>().Init(levelName2, isWorkshop2, true, onClickAction2, onDeleteAction2);
				gameObject2.SetActive(true);
			}
			return true;
		}

		private void OnUploadClicked()
		{
			MapSizeHandler.Instance.mapSizeFrame.root.gameObject.SetActive(false);
			AspectFix component = ScreenshotHandler.Instance.GetCamera().GetComponent<AspectFix>();
			component.scale = true;
			component.UpdateSize();
			bool gridVisibility = m_LevelCreator.GetGridVisibility();
			m_LevelCreator.ShowGrid(false);
			string newSaveName = m_SaveNameInputField.text.Trim();
			SaveNameResultEnum saveNameResultEnum = ValidateLevelName(newSaveName);
			if (!SteamManager.Initialized)
			{
				saveNameResultEnum = SaveNameResultEnum.Steam;
			}
			if (saveNameResultEnum == SaveNameResultEnum.Ok)
			{
				if (EditorLoadSave.CheckIfPublishNew())
				{
					Action yesAction = () =>
					{
						MapSizeHandler.Instance.mapSizeFrame.root.gameObject.SetActive(false);
						LevelEditorInputManager.SetNewInputState(false, false);
						m_LoadSave.PublishNew(newSaveName);
					};
					Action noAction = () =>
					{
						MapSizeHandler.Instance.mapSizeFrame.root.gameObject.SetActive(false);
						LevelEditorInputManager.SetNewInputState(false, false);
						m_LoadSave.PublishUpdate(newSaveName);
					};
					Action cancelAction = () =>
					{
					};
					DialougePanelUI.Instance.GiveChoice("Update Existing or Upload New Map: " + newSaveName + " ?", yesAction, "PublishNew", noAction, "Update Existing", cancelAction, "Cancel");
				}
				else
				{
					LevelEditorInputManager.SetNewInputState(false, false);
					m_LoadSave.PublishNew(newSaveName);
				}
			}
			else
			{
				Debug.Log("SaveName Error: " + saveNameResultEnum);
				DialougePanelUI.Instance.Prompt("Upload Error: " + GetErrorMessage(saveNameResultEnum));
			}
			m_LevelCreator.ShowGrid(gridVisibility);
			MapSizeHandler.Instance.mapSizeFrame.root.gameObject.SetActive(true);
		}

		private string GetErrorMessage(SaveNameResultEnum errorEnum)
		{
			switch (errorEnum)
			{
			case SaveNameResultEnum.Ok:
				return string.Empty;
			case SaveNameResultEnum.Empty:
				return "Name Is Empty!";
			case SaveNameResultEnum.InvalidCharacters:
				return "Name Contains Invalid Characters!";
			case SaveNameResultEnum.Steam:
				return "Steam Is Not Initialized!";
			default:
				return "Error: " + errorEnum;
			}
		}
	}
}
