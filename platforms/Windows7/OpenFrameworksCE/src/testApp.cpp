#include "testApp.h"
//--------------------------------------------------------------
void testApp::setup()
{
	colorEvent = INVALID_HANDLE_VALUE;
	HRESULT hr;
	int iSensorCount = 0;
	hr = NuiGetSensorCount(&iSensorCount);
  if (FAILED(hr))
		return;
	hr = NuiCreateSensorByIndex(0, &pKinectSensor);
	hr = pKinectSensor->NuiStatus();
	if(hr != S_OK)
	{
		hr = S_OK;
	}
	//|NUI_INITIALIZE_FLAG_USES_SKELETON
	hr = pKinectSensor->NuiInitialize(NUI_INITIALIZE_FLAG_USES_COLOR);

	if(hr == S_OK)
	{
		hr = S_OK;
	}

	colorEvent = CreateEventW(NULL, TRUE, FALSE, NULL);

	hr= pKinectSensor->NuiImageStreamOpen(
		NUI_IMAGE_TYPE_COLOR,
		NUI_IMAGE_RESOLUTION_640x480,
		0,
		2,
		colorEvent,
		&colorStreamHandle);

	if(hr != S_OK)
	{
		hr = S_OK;
	}

	//hr = NuiSkeletonTrackingEnable(skeletonEvent, 0);
	//if(hr == S_OK)
	//{
	//	hr = S_OK;
	//}
	colorText.allocate(640, 480, GL_RGBA);
	hr = NuiSkeletonTrackingEnable(skeletonEvent, 0);
}

//--------------------------------------------------------------
void testApp::update()
{
	//ofPoint handLocation = this->getRightHand();
	//std::cout << handLocation << std::endl;
	if(pKinectSensor == NULL)
		return;
	if (WAIT_OBJECT_0 == WaitForSingleObject(colorEvent, 0))
		ProcessColor();
}

void testApp::exit()
{
	if (pKinectSensor)
		pKinectSensor->NuiShutdown();

	if (colorEvent != INVALID_HANDLE_VALUE)
  {
		CloseHandle(colorEvent);
  }

	if(pKinectSensor != NULL)
	{
		pKinectSensor->Release();
		pKinectSensor = NULL;
	}
}

//--------------------------------------------------------------
void testApp::draw()
{
	colorText.draw(0,0);
}		

void testApp::ProcessColor()
{
	NUI_IMAGE_FRAME imageFrame;

	// Attempt to get the color frame
	pKinectSensor->NuiImageStreamGetNextFrame(colorStreamHandle, 0, &imageFrame);

	INuiFrameTexture * pTexture = imageFrame.pFrameTexture;
	NUI_LOCKED_RECT LockedRect;
	
	// Lock the frame data so the Kinect knows not to modify it while we're reading it
  pTexture->LockRect(0, &LockedRect, NULL, 0);

	// Make sure we've received valid data
  if (LockedRect.Pitch != 0)
  {
		//640,480,OF_IMAGE_COLOR
		
		//pixels1 = static_cast<BYTE *>(LockedRect.pBits);
		//unsigned char *pix = static_cast<BYTE *>(LockedRect.pBits);
		//int size=0;
		//int size2 = 0;
		//while(size < (LockedRect.size))
		//{

		//	pix[size2]   = 1;	  //colorPixel[size] =   1;
		//	pix[size2+1] = 1;   //colorPixel[size+1] = 1;
		//	pix[size2+1] = 1;   //colorPixel[size+1] = 1;
		//	size+=3;
		//	size2+=4;
		//}

		colorText.loadData(LockedRect.pBits, 640, 480, GL_BGRA);
	}
	pTexture->UnlockRect(0);
	// Release the frame
  pKinectSensor->NuiImageStreamReleaseFrame(colorStreamHandle, &imageFrame);
}


ofPoint testApp::getRightHand()
{
	ofPoint p1(-1, -1, -1);
	if (SUCCEEDED(NuiSkeletonGetNextFrame(0, &skeletonFrame)))
        {	
			for (int i = 0; i < NUI_SKELETON_COUNT ; i++)
				{
					const NUI_SKELETON_DATA & skeleton = skeletonFrame.SkeletonData[i];
					switch (skeleton.eTrackingState)
						{
							NuiTransformSmooth(&skeletonFrame, NULL);
							case NUI_SKELETON_TRACKED:
								float x=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_HAND_RIGHT].x,-2.2,2.2,0,4.4);
								float y=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_HAND_RIGHT].y*-1,-1.6,1.6,0,3.2);
								p1.set(x/4.4*1024,y/3.2*768);
								//float x=skeleton.SkeletonPositions[NUI_SKELETON_POSITION_HAND_RIGHT].x;
								//float y=skeleton.SkeletonPositions[NUI_SKELETON_POSITION_HAND_RIGHT].y;
								//p1.set(x,y);
								return p1;
								break;							
						}
				}
		}
}

void testApp::getSkeleton(float* skeletonX,float* skeletonY)
{
	if (SUCCEEDED(NuiSkeletonGetNextFrame(0, &skeletonFrame)))
        {	
			for (int i = 0; i < NUI_SKELETON_COUNT ; i++)
				{
					const NUI_SKELETON_DATA & skeleton = skeletonFrame.SkeletonData[i];
					switch (skeleton.eTrackingState)
						{
							NuiTransformSmooth(&skeletonFrame, NULL);
							case NUI_SKELETON_TRACKED:
								{float x=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_HEAD].x,-2.2,2.2,0,4.4);
								float y=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_HEAD].y*-1,-1.6,1.6,0,3.2);
								skeletonX[0]=x/4.4*1024;
								skeletonY[0]=y/3.2*768;

								x=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_CENTER].x,-2.2,2.2,0,4.4);
								y=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_CENTER].y*-1,-1.6,1.6,0,3.2);
								skeletonX[1]=x/4.4*1024;
								skeletonY[1]=y/3.2*768;

								x=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_LEFT].x,-2.2,2.2,0,4.4);
								y=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_LEFT].y*-1,-1.6,1.6,0,3.2);
								skeletonX[2]=x/4.4*1024;
								skeletonY[2]=y/3.2*768;

								x=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_ELBOW_LEFT].x,-2.2,2.2,0,4.4);
								y=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_ELBOW_LEFT].y*-1,-1.6,1.6,0,3.2);
								skeletonX[3]=x/4.4*1024;
								skeletonY[3]=y/3.2*768;

								x=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_WRIST_LEFT].x,-2.2,2.2,0,4.4);
								y=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_WRIST_LEFT].y*-1,-1.6,1.6,0,3.2);
								skeletonX[4]=x/4.4*1024;
								skeletonY[4]=y/3.2*768;

								x=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_HAND_LEFT].x,-2.2,2.2,0,4.4);
								y=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_HAND_LEFT].y*-1,-1.6,1.6,0,3.2);
								skeletonX[5]=x/4.4*1024;
								skeletonY[5]=y/3.2*768;

								x=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_HAND_RIGHT].x,-2.2,2.2,0,4.4);
								y=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_HAND_RIGHT].y*-1,-1.6,1.6,0,3.2);
								skeletonX[6]=x/4.4*1024;
								skeletonY[6]=y/3.2*768;

								x=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_WRIST_RIGHT].x,-2.2,2.2,0,4.4);
								y=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_WRIST_RIGHT].y*-1,-1.6,1.6,0,3.2);
								skeletonX[7]=x/4.4*1024;
								skeletonY[7]=y/3.2*768;

								x=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_ELBOW_RIGHT].x,-2.2,2.2,0,4.4);
								y=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_ELBOW_RIGHT].y*-1,-1.6,1.6,0,3.2);
								skeletonX[8]=x/4.4*1024;
								skeletonY[8]=y/3.2*768;

								x=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_RIGHT].x,-2.2,2.2,0,4.4);
								y=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_RIGHT].y*-1,-1.6,1.6,0,3.2);
								skeletonX[9]=x/4.4*1024;
								skeletonY[9]=y/3.2*768;

								x=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_SPINE].x,-2.2,2.2,0,4.4);
								y=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_SPINE].y*-1,-1.6,1.6,0,3.2);
								skeletonX[10]=x/4.4*1024;
								skeletonY[10]=y/3.2*768;

								x=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_HIP_LEFT].x,-2.2,2.2,0,4.4);
								y=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_HIP_LEFT].y*-1,-1.6,1.6,0,3.2);
								skeletonX[11]=x/4.4*1024;
								skeletonY[11]=y/3.2*768;

								x=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_KNEE_LEFT].x,-2.2,2.2,0,4.4);
								y=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_KNEE_LEFT].y*-1,-1.6,1.6,0,3.2);
								skeletonX[12]=x/4.4*1024;
								skeletonY[12]=y/3.2*768;

								x=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_ANKLE_LEFT].x,-2.2,2.2,0,4.4);
								y=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_ANKLE_LEFT].y*-1,-1.6,1.6,0,3.2);
								skeletonX[13]=x/4.4*1024;
								skeletonY[13]=y/3.2*768;

								x=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_FOOT_LEFT].x,-2.2,2.2,0,4.4);
								y=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_FOOT_LEFT].y*-1,-1.6,1.6,0,3.2);
								skeletonX[14]=x/4.4*1024;
								skeletonY[14]=y/3.2*768;

								x=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_FOOT_RIGHT].x,-2.2,2.2,0,4.4);
								y=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_FOOT_RIGHT].y*-1,-1.6,1.6,0,3.2);
								skeletonX[15]=x/4.4*1024;
								skeletonY[15]=y/3.2*768;

								x=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_ANKLE_RIGHT].x,-2.2,2.2,0,4.4);
								y=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_ANKLE_RIGHT].y*-1,-1.6,1.6,0,3.2);
								skeletonX[16]=x/4.4*1024;
								skeletonY[16]=y/3.2*768;

								x=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_KNEE_RIGHT].x,-2.2,2.2,0,4.4);
								y=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_KNEE_RIGHT].y*-1,-1.6,1.6,0,3.2);
								skeletonX[17]=x/4.4*1024;
								skeletonY[17]=y/3.2*768;

								x=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_HIP_RIGHT].x,-2.2,2.2,0,4.4);
								y=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_HIP_RIGHT].y*-1,-1.6,1.6,0,3.2);
								skeletonX[18]=x/4.4*1024;
								skeletonY[18]=y/3.2*768;

								x=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_HIP_CENTER].x,-2.2,2.2,0,4.4);
								y=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_HIP_CENTER].y*-1,-1.6,1.6,0,3.2);
								skeletonX[19]=x/4.4*1024;
								skeletonY[19]=y/3.2*768;								

								x=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_HIP_CENTER].x,-2.2,2.2,0,4.4);
								y=ofMap(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_HIP_CENTER].y*-1,-1.6,1.6,0,3.2);
								skeletonX[20]=x/4.4*1024;
								skeletonY[20]=y/3.2*768;}
								ofLog(OF_LOG_NOTICE,"tracked");
								break;
						/*case NUI_SKELETON_NOT_TRACKED:
								skeletonX[1]=-1;
								skeletonY[1]=-1;
								ofLog(OF_LOG_NOTICE,"NOT tracked");*/
						}

				}
		}	
}

//--------------------------------------------------------------
void testApp::keyPressed(int key){

}

//--------------------------------------------------------------
void testApp::keyReleased(int key){

}

//--------------------------------------------------------------
void testApp::mouseMoved(int x, int y ){

}

//--------------------------------------------------------------
void testApp::mouseDragged(int x, int y, int button){

}

//--------------------------------------------------------------
void testApp::mousePressed(int x, int y, int button){

}

//--------------------------------------------------------------
void testApp::mouseReleased(int x, int y, int button){

}

//--------------------------------------------------------------
void testApp::windowResized(int w, int h){

}

//--------------------------------------------------------------
void testApp::gotMessage(ofMessage msg)
{

}

//--------------------------------------------------------------
void testApp::dragEvent(ofDragInfo dragInfo){ 

}