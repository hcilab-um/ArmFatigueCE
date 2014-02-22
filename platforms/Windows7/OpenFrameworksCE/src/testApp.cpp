#include "testApp.h"

//--------------------------------------------------------------
void testApp::setup()
{
	engineCE = new FatigueEngine();
	engineCE->SetGender(Male);

	colorEvent = INVALID_HANDLE_VALUE;
	HRESULT hr;
	int iSensorCount = 0;
	hr = NuiGetSensorCount(&iSensorCount);

  if (FAILED(hr))
		return;
	
	hr = NuiCreateSensorByIndex(0, &pKinectSensor);
	if (FAILED(hr))
		return;

	hr = pKinectSensor->NuiStatus();
  if (FAILED(hr))
		return;

	hr = pKinectSensor->NuiInitialize(NUI_INITIALIZE_FLAG_USES_COLOR|NUI_INITIALIZE_FLAG_USES_SKELETON);
  if (FAILED(hr))
		return;

	colorEvent = CreateEventW(NULL, TRUE, FALSE, NULL);
	skeletonEvent = CreateEventW(NULL, TRUE, FALSE, NULL);

	hr = pKinectSensor->NuiImageStreamOpen(
		NUI_IMAGE_TYPE_COLOR,
		NUI_IMAGE_RESOLUTION_640x480,
		0,
		2,
		colorEvent,
		&colorStreamHandle);
	if (FAILED(hr))
		return;

	hr = NuiSkeletonTrackingEnable(skeletonEvent, 0);
  if (FAILED(hr))
		return;
	skeleton = NUI_SKELETON_DATA();
	skeleton.eTrackingState = NUI_SKELETON_NOT_TRACKED;

	colorText.allocate(IMAGE_WIDTH, IMAGE_HEIGHT, GL_RGBA);
}

//--------------------------------------------------------------
void testApp::update()
{
	if(pKinectSensor == NULL)
		return;
	if (WAIT_OBJECT_0 == WaitForSingleObject(colorEvent, 0))
		ProcessColor();
	if ( WAIT_OBJECT_0 == WaitForSingleObject(skeletonEvent, 0) )
		ProcessSkeleton();
}

void testApp::exit()
{
	if (pKinectSensor)
		pKinectSensor->NuiShutdown();

	if (colorEvent != INVALID_HANDLE_VALUE)
  {
		CloseHandle(colorEvent);
  }

	if (skeletonEvent && skeletonEvent != INVALID_HANDLE_VALUE)
		CloseHandle(skeletonEvent);

	delete engineCE;
	
	if(pKinectSensor != NULL)
	{
		pKinectSensor->Release();
		pKinectSensor = NULL;
	}

}

//--------------------------------------------------------------
void testApp::draw()
{

	ofSetColor(ofColor::white);
	colorText.draw(0, 0);
	ofSetColor(ofColor::green);
	ofSetLineWidth(5);
	DrawSkeleton(skeleton);
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
		colorText.loadData(LockedRect.pBits, IMAGE_WIDTH, IMAGE_HEIGHT, GL_BGRA);

	pTexture->UnlockRect(0);

	// Release the frame
  pKinectSensor->NuiImageStreamReleaseFrame(colorStreamHandle, &imageFrame);

}

void testApp::ProcessSkeleton()
{
	NUI_SKELETON_FRAME skeletonFrame = {0};
	HRESULT hr = pKinectSensor->NuiSkeletonGetNextFrame(0, &skeletonFrame);
	if (FAILED(hr))
    return;
	
	pKinectSensor->NuiTransformSmooth(&skeletonFrame, NULL);
	ofSetBackgroundColor(0,0,0);
			
	skeleton = NUI_SKELETON_DATA();
	skeleton.eTrackingState = NUI_SKELETON_NOT_TRACKED;

	for (int i = 0; i < NUI_SKELETON_COUNT; ++i)
  {
		if(skeletonFrame.SkeletonData[i].eTrackingState == NUI_SKELETON_TRACKED)
		{
			skeleton = skeletonFrame.SkeletonData[i];
			break;
		}
	}
}

void testApp::DrawSkeleton(const NUI_SKELETON_DATA & skel)
{
	if(skel.eTrackingState != NUI_SKELETON_TRACKED)
		return;
		
	// Render Torso
  DrawBone(skel, NUI_SKELETON_POSITION_HEAD, NUI_SKELETON_POSITION_SHOULDER_CENTER);
  DrawBone(skel, NUI_SKELETON_POSITION_SHOULDER_CENTER, NUI_SKELETON_POSITION_SHOULDER_LEFT);
  DrawBone(skel, NUI_SKELETON_POSITION_SHOULDER_CENTER, NUI_SKELETON_POSITION_SHOULDER_RIGHT);
  DrawBone(skel, NUI_SKELETON_POSITION_SHOULDER_CENTER, NUI_SKELETON_POSITION_SPINE);
  DrawBone(skel, NUI_SKELETON_POSITION_SPINE, NUI_SKELETON_POSITION_HIP_CENTER);
  DrawBone(skel, NUI_SKELETON_POSITION_HIP_CENTER, NUI_SKELETON_POSITION_HIP_LEFT);
  DrawBone(skel, NUI_SKELETON_POSITION_HIP_CENTER, NUI_SKELETON_POSITION_HIP_RIGHT);

  // Left Arm
  DrawBone(skel, NUI_SKELETON_POSITION_SHOULDER_LEFT, NUI_SKELETON_POSITION_ELBOW_LEFT);
  DrawBone(skel, NUI_SKELETON_POSITION_ELBOW_LEFT, NUI_SKELETON_POSITION_WRIST_LEFT);
  DrawBone(skel, NUI_SKELETON_POSITION_WRIST_LEFT, NUI_SKELETON_POSITION_HAND_LEFT);

  // Right Arm
  DrawBone(skel, NUI_SKELETON_POSITION_SHOULDER_RIGHT, NUI_SKELETON_POSITION_ELBOW_RIGHT);
  DrawBone(skel, NUI_SKELETON_POSITION_ELBOW_RIGHT, NUI_SKELETON_POSITION_WRIST_RIGHT);
  DrawBone(skel, NUI_SKELETON_POSITION_WRIST_RIGHT, NUI_SKELETON_POSITION_HAND_RIGHT);

  // Left Leg
  DrawBone(skel, NUI_SKELETON_POSITION_HIP_LEFT, NUI_SKELETON_POSITION_KNEE_LEFT);
  DrawBone(skel, NUI_SKELETON_POSITION_KNEE_LEFT, NUI_SKELETON_POSITION_ANKLE_LEFT);
  DrawBone(skel, NUI_SKELETON_POSITION_ANKLE_LEFT, NUI_SKELETON_POSITION_FOOT_LEFT);

  // Right Leg
  DrawBone(skel, NUI_SKELETON_POSITION_HIP_RIGHT, NUI_SKELETON_POSITION_KNEE_RIGHT);
  DrawBone(skel, NUI_SKELETON_POSITION_KNEE_RIGHT, NUI_SKELETON_POSITION_ANKLE_RIGHT);
  DrawBone(skel, NUI_SKELETON_POSITION_ANKLE_RIGHT, NUI_SKELETON_POSITION_FOOT_RIGHT);
}

void testApp::DrawBone(const NUI_SKELETON_DATA & skel, NUI_SKELETON_POSITION_INDEX joint0, NUI_SKELETON_POSITION_INDEX joint1)
{
	ofPolyline pol = ofPolyline();
	NUI_SKELETON_POSITION_TRACKING_STATE joint0State = skel.eSkeletonPositionTrackingState[joint0];
  NUI_SKELETON_POSITION_TRACKING_STATE joint1State = skel.eSkeletonPositionTrackingState[joint1];
	if (joint0State == NUI_SKELETON_POSITION_NOT_TRACKED || joint1State == NUI_SKELETON_POSITION_NOT_TRACKED)
		return;

	if (joint0State == NUI_SKELETON_POSITION_INFERRED && joint1State == NUI_SKELETON_POSITION_INFERRED)
		return;

	ofPoint point0 = SkeletonToScreen(skel.SkeletonPositions[joint0], IMAGE_WIDTH, IMAGE_HEIGHT);
	ofPoint point1 = SkeletonToScreen(skel.SkeletonPositions[joint1], IMAGE_WIDTH, IMAGE_HEIGHT);
	pol.addVertex(point0);
	pol.addVertex(point1);

	pol.draw();
}

ofPoint testApp::SkeletonToScreen(Vector4 skeletonPoint, const int width, const int height)
{
	LONG x, y;
  USHORT depth;

  // Calculate the skeleton's position on the screen
  // NuiTransformSkeletonToDepthImage returns coordinates in NUI_IMAGE_RESOLUTION_320x240 space
  NuiTransformSkeletonToDepthImage(skeletonPoint, &x, &y, &depth);
	float screenPointX = static_cast<float>(x * width) / 320;
  float screenPointY = static_cast<float>(y * height) / 240;
	return ofPoint(screenPointX, screenPointY);
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
void testApp::windowResized(int w, int h)
{
}

//--------------------------------------------------------------
void testApp::gotMessage(ofMessage msg)
{

}

//--------------------------------------------------------------
void testApp::dragEvent(ofDragInfo dragInfo){ 

}