using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VIewer : MonoBehaviour
{
    public ParticleSystem[] Particles;
    public int showNum = 0;
    public GameObject ShowPos;
    public Text _fxNameText;
   
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < Particles.Length; i++)
        {
            Particles[i].gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        showNum = Mathf.Clamp(showNum, 0, Particles.Length - 1);
        _fxNameText.text = Particles[showNum].gameObject.name;
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Previous();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Next();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayFX();
        }
    }

    public void Next()
    {
        if (showNum != Particles.Length - 1)
        {
            Particles[showNum].gameObject.SetActive(false);
            showNum += 1;
            Particles[showNum].gameObject.transform.position = ShowPos.transform.position;
            Particles[showNum].gameObject.SetActive(true);
        }
        else
        {
            Particles[showNum].gameObject.SetActive(false);
            showNum = 0;
            Particles[showNum].gameObject.transform.position = ShowPos.transform.position;
            Particles[showNum].gameObject.SetActive(true);

        }

    }

    public void Previous()
    {
        if (showNum != 0)
        {
            Particles[showNum].gameObject.SetActive(false);
            showNum -= 1;
            Particles[showNum].gameObject.transform.position = ShowPos.transform.position;
            Particles[showNum].gameObject.SetActive(true);
        }
        else
        {
            Particles[showNum].gameObject.SetActive(false);
            showNum = Particles.Length -1;
            Particles[showNum].gameObject.transform.position = ShowPos.transform.position;
            Particles[showNum].gameObject.SetActive(true);
        }
        
    }

    public void PlayFX()
    {
       Particles[showNum].gameObject.transform.position = ShowPos.transform.position;
       Particles[showNum].gameObject.SetActive(true);
       Particles[showNum].Play();


    }


}
