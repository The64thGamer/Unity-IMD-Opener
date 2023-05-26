using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ImdOpener : MonoBehaviour
{
    public string filePath;
    public bool readFileDebug;
    byte[] imdFile;
    IMD currentIMD;

    private void FixedUpdate()
    {
        if(readFileDebug)
        {
            readFileDebug = false;
            LoadIMDFile();
        }
    }

    public void LoadIMDFile()
    {
        //Setup
        ReadFileIntoByte();

        if(imdFile == null)
        {
            Debug.Log("IMD Opener: No File, Error");
            return;
        }

        //Header
        byte[] byteHeader = new byte[29];

        for (int i = 0; i < byteHeader.Length; i++)
        {
            byteHeader[i] = imdFile[i];
        }
        
        currentIMD.header = System.Text.Encoding.ASCII.GetString(byteHeader);

        //Comment
        List<byte> commentBytes = new List<byte>();

        int e = 29;

        while (true)
        {
            //Check for ASCII Terminator
            if (imdFile[e] == 26)
            {
                break;
            }
            else
            {
                commentBytes.Add(imdFile[e]);
            }
            e++;
        }

        currentIMD.comment = System.Text.Encoding.ASCII.GetString(commentBytes.ToArray());

        //Debug
        ReadAllContents();
    }

    public void ReadFileIntoByte()
    {
        imdFile = File.ReadAllBytes(filePath);
    }

    public void ReadAllContents()
    {
        Debug.Log("IMD Opener: Header - '" + currentIMD.header + "'");
        Debug.Log("IMD Opener: Comment - '" + currentIMD.comment + "'");

    }
}

public struct IMD
{
    public string header;
    public string comment;
}
