using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessageController : MonoBehaviour
{
    int MAX_FOV = 100;
    int MIN_FOV = 20;
    int DEFAULT_FOV = 60;
    int FOV_DURATION = 10;
    int FOV_STEP = 1;

    int ROT_DURATION = 10;
    int ROT_STEP = 1;

    Quaternion[] DEFAULT_CAM_ROT;


    [SerializeField]
    GameObject cam0;
    [SerializeField]
     GameObject cam1;
    [SerializeField]
    GameObject cam2;
    [SerializeField]
    GameObject cam3;

    private GameObject[] cams;
    private int cam_cursor = 0;
    private bool is_cam_moving = false;

    [SerializeField]
    GameObject phoneUI;

    private MainUIView mainUIView;

    private enum CamZoom{
        IN, OUT
    }
    private enum CamDirection{
        RIGHT,LEFT,UP,DOWN
    }

    void Start(){
        mainUIView = phoneUI.GetComponent<MainUIView>();

        cams = new GameObject[]{cam0,cam1,cam2,cam3};
        DEFAULT_CAM_ROT = new Quaternion[cams.Length];
        for (int i = 0; i < cams.Length; i++){
            DEFAULT_CAM_ROT[i] = cams[i].transform.rotation;
        }
        SwitchCAM(0);
    }

    public void SwitchCmd(MsgFormat data){

        var msgs = data.msg.Split(' ');

        switch(msgs[0]){
            // cameraのスイッチ
            case "%switch":
                cam_cursor ++;
                Debug.Log(cam_cursor);
                if(cam_cursor == cams.Length){
                    cam_cursor = 0;
                    SwitchCAM(cam_cursor);
                }
                else{
                    SwitchCAM(cam_cursor);
                }
                break;
            
            // cameraのズーム
            case "%zoomin":
                if(!is_cam_moving)StartCoroutine(ZoomCAM(cam_cursor, CamZoom.IN));
                break;
            case "%zoomout":
                if(!is_cam_moving)StartCoroutine(ZoomCAM(cam_cursor, CamZoom.OUT));
                break;
            
            // activeなカメラの方向を操作する。
            case "%right":
                if(!is_cam_moving)StartCoroutine(RotCAM(cam_cursor, CamDirection.RIGHT));
                break;
            case "%left":
                if(!is_cam_moving)StartCoroutine(RotCAM(cam_cursor, CamDirection.LEFT));
                break;
            case "%up":
                if(!is_cam_moving)StartCoroutine(RotCAM(cam_cursor, CamDirection.UP));
                break;
            case "%down":
                if(!is_cam_moving)StartCoroutine(RotCAM(cam_cursor, CamDirection.DOWN));
                break;

            // テキストをobj化
            case "%obj":
                var obj = FlyingText.GetObject(msgs[1]);
                obj.AddComponent<TextObject>();               
                break;
            
            // UIのchat欄に追加
            default:
                if(!data.isOwner){
                    if(data.msg.StartsWith("%")){
                        //使えないコマンド
                        var nonusable = FlyingText.GetObject(data.msg + "は使えません。");
                        nonusable.AddComponent<TextObject>();
                    }
                    else{
                        mainUIView.UpdateChatText(data.msg);
                    }
                }
                break;
        }
    }

    private void SwitchCAM(int cursor){
        for(int i = 0; i < cams.Length; i++){
            if (i == cursor){
                cams[i].SetActive(true);
            }
            else{
                cams[i].SetActive(false);

                //現在activeでないカメラはデフォルト状態に戻す。
                cams[i].GetComponent<Camera>().fieldOfView = DEFAULT_FOV;
                cams[i].transform.rotation = DEFAULT_CAM_ROT[i];
            }
        }
    }
    private IEnumerator ZoomCAM(int cam_cursor, CamZoom direction){
        is_cam_moving = true;
        var nowFov = cams[cam_cursor].GetComponent<Camera>().fieldOfView;
        for(int i = 0; i < FOV_DURATION; i++ ){
            switch(direction){
                case CamZoom.IN:
                    cams[cam_cursor].GetComponent<Camera>().fieldOfView = Mathf.Clamp(nowFov - FOV_STEP, MIN_FOV, MAX_FOV);
                    break;
                case CamZoom.OUT:
                    cams[cam_cursor].GetComponent<Camera>().fieldOfView = Mathf.Clamp(nowFov + FOV_STEP, MIN_FOV, MAX_FOV);
                    break;
                default:
                    break;
            }
            nowFov = cams[cam_cursor].GetComponent<Camera>().fieldOfView;
            yield return new WaitForSeconds(0.05f);
        }
        is_cam_moving = false;
    }
    private IEnumerator RotCAM(int cam_cursor, CamDirection direction){
        is_cam_moving = true;
        for(int i = 0; i < ROT_DURATION; i++){
            switch (direction){
                case CamDirection.RIGHT:
                    cams[cam_cursor].transform.Rotate(0f,ROT_STEP,0f);
                    break;
                case CamDirection.LEFT:
                    cams[cam_cursor].transform.Rotate(0f,ROT_STEP * -1,0f);
                    break;
                case CamDirection.UP:
                    cams[cam_cursor].transform.Rotate(ROT_STEP * -1,0f,0f);
                    break;
                case CamDirection.DOWN:
                    cams[cam_cursor].transform.Rotate(ROT_STEP,0f,0f);
                    break;
                default:
                    break;
            }
            yield return new WaitForSeconds(0.05f);
        }
        is_cam_moving = false;
    }
}