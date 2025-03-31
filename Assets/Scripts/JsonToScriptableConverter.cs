#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System;


public enum ConversionType
{
    Items,
    Dialogs
}

[Serializable]

public class DialogRowData
{
    public int? id;                     //int?�� Nullable<int>�� ��� ǥ���Դϴ�. �����ϸ� null ���� ���� �� �ִ� �������� �˴ϴ�
    public string characterName;         
    public string text;
    public int? nextId;
    public string portraitPath;
    public string choiceText;
    public int? choiceNextId;
}
public class JsonToScriptableConverter : EditorWindow
{
    private string jsonFilePath = "";                                    //JSON ���� ��� ���ڿ� ��
    private string outputFolder = "Assets/ScriptableObjects";      //��� SO ������ ��� ��
    private bool createDatabase = true;                                  //������ ���̽��� ��� �� �������� ���� bool ��
    private ConversionType conversionType = ConversionType.Items;


    [MenuItem("Tools/JSON to Scriptable Objects")]

    public static void ShowWindow()
    {
        GetWindow<JsonToScriptableConverter>("JSON to Scriptable Objects");
    }

    void OnGUI()
    {
        GUILayout.Label("JSON to Scriptable Object Converte", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (GUILayout.Button("Select JSON File"))
        {
            jsonFilePath = EditorUtility.OpenFilePanel("Select JSON File", "", "json");
        }
        EditorGUILayout.LabelField("Selected File:", jsonFilePath);
        EditorGUILayout.Space();

        //��ȯ Ÿ�� ����
        conversionType = (ConversionType)EditorGUILayout.EnumPopup("Conversion Type:", conversionType);

        //Ÿ�Կ� ���� �⺻ ��� ���� ����
        if (conversionType == ConversionType.Items )
        {
            outputFolder = "Assets/Scriptables/Items";
        }
        else if (conversionType == ConversionType.Dialogs )
        {
            outputFolder = "Assets/ScriptableObjects/Dialogs";
        }


        outputFolder = EditorGUILayout.TextField("outputFolder:", outputFolder);
        createDatabase = EditorGUILayout.Toggle("create Database Asset", createDatabase);
        EditorGUILayout.Space();

        if (GUILayout.Button("Convert to Scriptable Objects"))
        {
            if (string.IsNullOrEmpty(jsonFilePath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a JSON file firest!", "OK");
                return;
            }
            switch (conversionType)
            {
                case ConversionType.Items:
                ConvertJsonToItemScriptableObjects();
                    break; 
                case ConversionType.Dialogs:
                    ConvertJsonToDialogScriptableObjects();
                    break;
                
                   
            }




        }

    }

    private void ConvertJsonToItemScriptableObjects()    //JSON ������ ScriptableObject ���Ϸ� ��ȯ �����ִ� �Լ�
    {
        //���� ����
        if (!Directory.Exists(outputFolder))          //���� ��ġ�� Ȯ���ϰ� ������ ���� �Ѵ�.
        {
            Directory.CreateDirectory(outputFolder);
        }

        //JSON ���� ��
        string jsonText = File.ReadAllText(jsonFilePath); //JSON ������ �д´�/

        try
        {
            //JSON �Ľ�
            List<ItemData> itemsDataList = JsonConvert.DeserializeObject<List<ItemData>>(jsonText);

            List<ItemSO> createdItems = new List<ItemSO>();               //ItemSO ����Ʈ ����

            //�� ������ �����͸� ��ũ���ͺ� ������Ʈ�� ��ȯ
            foreach (var itemData in itemsDataList)
            {
                ItemSO itemSO = ScriptableObject.CreateInstance<ItemSO>();  //ItemSO ������ ����

                //������ ����
                itemSO.id = itemData.id;
                itemSO.itemName = itemData.itemName;
                itemSO.nameEng = itemData.nameEng;
                itemSO.description = itemData.description;

                //������ ��ȯ
                if (System.Enum.TryParse(itemData.itemTypeString, out ItemType parsedType))
                {
                    itemSO.itemType = parsedType;
                }
                else
                {
                    Debug.LogWarning($"������'{itemData.itemName}�� ��ȿ���� �ʴ� Ÿ�� : {itemData.itemTypeString}");
                }
                itemSO.price = itemData.price;
                itemSO.power = itemData.power;
                itemSO.level = itemData.level;
                itemSO.isStackable = itemData.isStackable;

                //������ �ε�(��ΰ� �ִ� ���)
                if (!string.IsNullOrEmpty(itemData.iconPath))
                {
                    itemSO.icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/{itemData.iconPath}.png");

                    if (itemSO.icon != null)
                    {
                        Debug.LogWarning($"������'{itemData.nameEng}'�� �������� ã�� ���� �����ϴ�. :{itemData.iconPath}");
                    }
                }

                //��ũ���ͺ� ������Ʈ ���� - ID�� 4�ڸ� ���ڷ� ������
                string assetPath = $"{outputFolder}/Item_{itemData.id.ToString("D4")}_{itemData.nameEng}.asset";
                AssetDatabase.CreateAsset(itemSO, assetPath);

                //���� �̸� ����
                itemSO.name = $"Item_{itemData.id.ToString("D4")}+{itemData.nameEng}";
                createdItems.Add(itemSO);

                EditorUtility.SetDirty(itemSO);
            }
            //�����ͺ��̽� ����
            if (createDatabase && createdItems.Count > 0)
            {
                ItemDatabaseSO database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
                database.items = createdItems;

                AssetDatabase.CreateAsset(database, $"{outputFolder}/ItemDatabase.asset");
                EditorUtility.SetDirty(database);

            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Sucess", $"Created{createdItems.Count} dialog scriptable objects!", "OK");












        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to Covert JSON: {e.Message}", "OK");
            Debug.LogError($"JSON ��ȯ ����: {e}");
        }
    }

    //��ȭ JSON�� ��ũ���ͺ� ��������� ��ȯ

    private void ConvertJsonToDialogScriptableObjects()
    {
        //���� ����
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        //JSON ���� �б�
        string jsonText = File.ReadAllText(jsonFilePath);

        try
        {
            //JSON �Ľ�
            List<DialogRowData> rowDataList = JsonConvert.DeserializeObject<List<DialogRowData>>(jsonText);

            //��ȭ ������ ����
            Dictionary<int, DialogSO> dialogMap = new Dictionary<int, DialogSO>();
            List<DialogSO> createDialogs = new List<DialogSO>();

            //1�ܰ�: ��ȭ �׸� ����
            foreach (var rowData in rowDataList)
            {
                //id �ִ� ���� ��ȭ�� ó��
                if (rowData.id.HasValue)
                {
                    DialogSO dialogSO = ScriptableObject.CreateInstance<DialogSO>();
                    //������ ����
                    dialogSO.id = rowData.id.Value;
                    dialogSO.characterName = rowData.characterName;
                    dialogSO.text = rowData.text;
                    dialogSO.nextild = rowData.nextId.HasValue ? rowData.nextId.Value : -1;
                    dialogSO.portraitPath = rowData.portraitPath;
                    dialogSO.choices = new List<DialogChoiceSO>();

                    //�ʻ�ȭ �ε� (��ΰ� �ִ°��)
                    if (!string.IsNullOrEmpty(rowData.portraitPath))
                    {
                        dialogSO.portrait = Resources.Load<Sprite>(rowData.portraitPath);
                        if (dialogSO.portrait = null)
                        {
                            Debug.LogWarning($"��ȭ {rowData.id}�� �ʻ�ȭ�� ã�� �� �����ϴ�.");
                        }
                    }
                    //dialogMap�� �߰�
                    dialogMap[dialogSO.id] = dialogSO;
                    createDialogs.Add(dialogSO);
                }
            }

            //2�ܰ�: ������ �׸� ó�� �� ����
            foreach (var rowData in rowDataList)
            {
                //id�� ���� choiceText�� �ִ� ���� �������� ó��
                if (!rowData.id.HasValue && !string.IsNullOrEmpty(rowData.choiceText) && rowData.choiceNextId.HasValue)
                {
                    //������ ���� ID�� �θ� ID�� ��� (���ӵǴ� �������� ���)
                    int parentId = -1;

                    //�� ������ �ٷ� ���� �ִ� ��ȭ (id�� �ִ� �׸�)�� ã��
                    int currentIndex = rowDataList.IndexOf(rowData);
                    for (int i = currentIndex - 1; i >= 0; i--)
                    {
                        if (rowDataList[i].id.HasValue)
                        {
                            parentId = rowDataList[i].id.Value;
                            break;
                        }

                        //�θ� ID�� ã�� ���߰ų� �θ� ID�� -1�� ��� (ù ��° �׸�)
                        if (parentId == -1)
                        {
                            Debug.LogWarning($"������ '{rowData.choiceText}'�� �θ� ��ȭ�� ã�� �� �����ϴ�.");
                        }

                        if (dialogMap.TryGetValue(parentId, out DialogSO parentDialog))
                        {
                            DialogChoiceSO choiceSO = ScriptableObject.CreateInstance<DialogChoiceSO>();
                            choiceSO.text = rowData.choiceText;
                            choiceSO.nextld = rowData.choiceNextId.Value;

                            //������ ���� ����
                            string choiceAssetPath = $"{outputFolder}/Choice_{parentId}_{parentDialog.choices.Count + 1}.asset";
                            AssetDatabase.CreateAsset(choiceSO, choiceAssetPath);
                            EditorUtility.SetDirty(choiceSO);

                            parentDialog.choices.Add(choiceSO);

                        }
                        else
                        {
                            Debug.LogWarning($"������ '{rowData.choiceText}'�� ������ ��ȭ (ID : {parentId})�� ã�� �� �����ϴ�.");

                        }
                    }




                    //3�ܰ�: ��ȭ ��ũ���ͺ� ������Ʈ ����
                    foreach (var dialog in createDialogs)
                    {
                        //��ũ���ͺ� ������Ʈ ���� - ID�� 4�ڸ� ���ڷ� ������
                        string assetPath = $"{outputFolder}/Dialog_{dialog.id.ToString("D4")}.asset";
                        AssetDatabase.CreateAsset(dialog, assetPath);

                        //���� �̸� ����
                        dialog.name = $"Dialog_{dialog.id.ToString("D4")}";

                        EditorUtility.SetDirty(dialog);
                    }

                    //������ ���̽� �ļ�
                    if (createDatabase && createDialogs.Count > 0)
                    {
                        DialogDatabaseSO database = ScriptableObject.CreateInstance<DialogDatabaseSO>();
                        database.dialogs = createDialogs;

                        AssetDatabase.CreateAsset(database, $"{outputFolder}/DialogDatabase.asset");
                        EditorUtility.SetDirty(database);

                    }
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    EditorUtility.DisplayDialog("Success", $"Created {createDialogs.Count} dialog scriptable objects!", "OK");
                }
            }
        }









        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to convert JSON: {e.Message}", "OK");
            Debug.LogError($"JSON ��ȯ ���� :{e}");
        }
    }
}



#endif


