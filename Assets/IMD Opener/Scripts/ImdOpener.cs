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

        //Track Reader
        while (true)
        {

        }

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
        for (int i = 0; i < currentIMD.tracks.Count; i++)
        {
            Debug.Log("IMD Opener: Reading Track #" + i);
;           Debug.Log("IMD Opener: Mode Value - '" + currentIMD.tracks[i].modeValue + "'");
            Debug.Log("IMD Opener: Cylinder - '" + currentIMD.tracks[i].cylinder + "'");
            Debug.Log("IMD Opener: Head - '" + currentIMD.tracks[i].head + "'");
            Debug.Log("IMD Opener: # Sectors In Track - '" + currentIMD.tracks[i].noSectorsInTrack + "'");
            Debug.Log("IMD Opener: Sector Size - '" + currentIMD.tracks[i].sectorSize + "'");
        }
    }
}

public struct IMD
{
    public string header;
    public string comment;
    public List<IMDTrack> tracks;
}

public struct IMDTrack
{
    public byte modeValue;
    public byte cylinder;
    public byte head;
    public byte noSectorsInTrack;
    public byte sectorSize;
}