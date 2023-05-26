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
                e++;
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
        int j = 0;
        currentIMD.tracks = new List<IMDTrack>();

        IMDTrack currentTrack = new IMDTrack();
        currentTrack.modeValue = imdFile[e];
        e++;
        currentTrack.cylinder = imdFile[e];
        e++;
        currentTrack.head = imdFile[e];
        e++;
        currentTrack.noSectorsInTrack = imdFile[e];
        e++;
        currentTrack.sectorSize = imdFile[e];
        e++;


        currentIMD.tracks.Add(currentTrack);
        j++;

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
            switch (currentIMD.tracks[i].modeValue)
            {
                case 0:
                    Debug.Log("IMD Opener: Mode Value - '500' kbps (FM)");
                    break;
                case 1:
                    Debug.Log("IMD Opener: Mode Value - '300' kbps (FM)");
                    break;
                case 2:
                    Debug.Log("IMD Opener: Mode Value - '250' kbps (FM)");
                    break;
                case 3:
                    Debug.Log("IMD Opener: Mode Value - '500' kbps (MFM)");
                    break;
                case 4:
                    Debug.Log("IMD Opener: Mode Value - '300' kbps (MFM)");
                    break;
                case 5:
                    Debug.Log("IMD Opener: Mode Value - '250' kbps (MFM)");
                    break;
                default:
                    Debug.LogError("IMD Opener: Mode Value outside of range 0-5 - '" + currentIMD.tracks[i].modeValue + "'");
                    break;
            }
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