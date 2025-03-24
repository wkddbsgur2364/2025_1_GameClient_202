#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Unity.VisualScripting.FullSerializer;


public class JsonToScriptableConverter : EditorWindow
{
    private string jsonFilePath = "";                                    //JSON ���� ��� ���ڿ� ��
    private string outputFolder = "Assets/ScriptableObjects/items";      //��� SO ������ ��� ��
    private bool createDatabase = true;                                  //������ ���̽��� ��� �� �������� ���� bool ��

    [MenuItem("Tools/JSON to Scriptable Objects")]

    public static void ShowWindow()
    {
        GetWindow<JsonToScriptableConverter>("JSON to Scriptable Objects");
    }

    void OnGUI()
    {
        GUILayout.Label("JSON to Scriptable Object Converte",EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if(GUILayout.Button("Select JSON File"))
        {
            jsonFilePath = EditorUtility.OpenFilePanel("Select JSON File", "", "json");
        }
        EditorGUILayout.LabelField("Selected File:", jsonFilePath);
        EditorGUILayout.Space();
        outputFolder=EditorGUILayout.TextField("outputFolder:",outputFolder);
        createDatabase=EditorGUILayout.Toggle("create Database Asset",createDatabase);
        EditorGUILayout.Space();

        if(GUILayout.Button("Convert to Scriptable Objects"))
        {
            if(string.IsNullOrEmpty(jsonFilePath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a JSON file firest!", "OK");
                return;
            }
            ConvertJsonToScriptableObjects();
        }

    }

    private void ConvertJsonToScriptableObjects()    //JSON ������ ScriptableObject ���Ϸ� ��ȯ �����ִ� �Լ�
    {
        //���� ����
        if(!Directory.Exists(outputFolder))          //���� ��ġ�� Ȯ���ϰ� ������ ���� �Ѵ�.
        {
            Directory.CreateDirectory(outputFolder);
        }

        //JSON ���� ��
        string jsonText=File.ReadAllText(jsonFilePath); //JSON ������ �д´�/

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

            EditorUtility.DisplayDialog("Sucess", $"Created{createdItems.Count} scriptable objects!", "OK");

        


            






            
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error",$"Failed to Covert JSON: { e.Message}","OK");
            Debug.LogError($"JSON ��ȯ ����: {e}");
        }
    }
}

#endif


