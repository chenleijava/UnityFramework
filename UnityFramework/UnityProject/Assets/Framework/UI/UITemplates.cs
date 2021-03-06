﻿using UnityEngine;
using System.Collections.Generic;
using System;

public class UITemplates<T>
    where T : MonoBehaviour, ITemplatable
{
    Dictionary<string, T> templates = new Dictionary<string, T>();

    Dictionary<string, Stack<T>> cache = new Dictionary<string, Stack<T>>();

    bool findTemplatesCalled;
    UnityEngine.Events.UnityAction<T> onCreateCallback;

    public UITemplates(UnityEngine.Events.UnityAction<T> onCreateCallback = null)
    {
        this.onCreateCallback = onCreateCallback;
    }

    public void FindTemplates()
    {
        findTemplatesCalled = true;

        Resources.FindObjectsOfTypeAll<T>().ForEach(x =>
        {
            Add(x.name, x, replace: true);
            x.gameObject.SetActive(false);
        });
    }

    public void ClearAll()
    {
        templates.Clear();
        ClearCache();
    }

    public void ClearCache()
    {
        cache.Keys.ForEach(x =>
        {
            ClearCache(x);
        });
        findTemplatesCalled = false;
    }

    public void ClearCache(string name)
    {
        if (!cache.ContainsKey(name))
        {
            return;
        }
        cache[name].ForEach(x =>
        {
            UnityEngine.Object.Destroy(x.gameObject);
        });
        cache[name].Clear();
        cache[name].TrimExcess();
    }

    public bool Exists(string name)
    {
        return templates.ContainsKey(name);
    }

    public T Get(string name)
    {
        if (!Exists(name))
        {
            DebugConsole.LogError("Not found template with name '" + name + "'");
        }

        return templates[name];
    }

    public void Delete(string name)
    {
        if (!Exists(name))
            return;

        templates.Remove(name);
        ClearCache(name);
    }

    public void Add(string name, T template, bool replace = true)
    {
        if (Exists(name))
        {
            if (!replace)
            {
                DebugConsole.LogError("Template with name '" + name + "' already exists.");
            }

            ClearCache(name);
            templates[name] = template;
        }
        else
        {
            templates.Add(name, template);
        }
        template.IsTemplate = true;
        template.TemplateName = name;
    }

    public T GetDuplicate(string name)
    {
        if (!findTemplatesCalled)
        {
            FindTemplates();
        }

        if ((!Exists(name)) || (templates[name] == null))
        {
            Debug.LogError("Not found template with name '" + name + "'");
        }

        T duplicate;
        if ((cache.ContainsKey(name)) && (cache[name].Count > 0))
        {
            duplicate = cache[name].Pop();
        }
        else
        {
            duplicate = UnityEngine.Object.Instantiate(templates[name]) as T;

            duplicate.TemplateName = name;
            duplicate.IsTemplate = false;

            if (onCreateCallback != null)
            {
                onCreateCallback(duplicate);
            }
        }

        if (templates[name].transform.parent != null)
        {
           // duplicate.transform.SetParent(templates[name].transform.parent);
        }


        return duplicate;
    }

    public void ReturnCache(T instance)
    {
        instance.gameObject.SetActive(false);

        if (!cache.ContainsKey(instance.TemplateName))
        {
            cache[instance.TemplateName] = new Stack<T>();
        }
        cache[instance.TemplateName].Push(instance);
    }
}
