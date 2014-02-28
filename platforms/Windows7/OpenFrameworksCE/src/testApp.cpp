#include "testApp.h"

//--------------------------------------------------------------
void testApp::setup()
{
	HRESULT hr = startKinect();
	if(FAILED(hr))
		return;
	
	startButton = ofxButton();
	stopButton = ofxButton();

	startButton.setup("Start", 100,50);
	startButton.addListener(this, &testApp::startButtonClick);
	startButton.setPosition(0, IMAGE_HEIGHT);
	startButton.setBackgroundColor(ofColor::black);
	startButton.setTextColor(ofColor::white);
	
	stopButton.setup("Stop", 100,50);
	stopButton.addListener(this, &testApp::stopButtonClick);
	stopButton.setPosition(150, IMAGE_HEIGHT);
	stopButton.setBackgroundColor(ofColor::black);
	stopButton.setTextColor(ofColor::white);
	labelCE.setup("CE: ", "0.00", 200, 50);
	labelCE.setPosition(300, IMAGE_HEIGHT);
	labelCE.setBackgroundColor(ofColor::black);
}

HRESULT testApp::startKinect()
{
	engineCE = new FatigueEngine();
	engineCE->SetGender(Male);
	engineCE->Reset();

	colorEvent = INVALID_HANDLE_VALUE;

	HRESULT hr;
	hr = NuiCreateSensorByIndex(0, &pKinectSensor);
	if (FAILED(hr))
	{
		ofLogError("ofGLUtils") << "Fail to get sensor";
		return hr;
	}

	hr = pKinectSensor->NuiStatus();
	if (FAILED(hr))
	{
		ofLogError("ofGLUtils") << "Kinect Sensor Status Error";
		return hr;
	}

	pKinectSensor->NuiInitialize(NUI_INITIALIZE_FLAG_USES_COLOR|NUI_INITIALIZE_FLAG_USES_SKELETON);

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
	{
		ofLogError("ofGLUtils") << "Fail to Initialize ColorFrame";
		return hr;
	}

	hr = NuiSkeletonTrackingEnable(skeletonEvent, 0);
  if (FAILED(hr))
	{
		ofLogError("ofGLUtils") << "Fail to Initialize SKeletonFrame";
		return hr;
	}

	skeleton = NUI_SKELETON_DATA();
	skeleton.eTrackingState = NUI_SKELETON_NOT_TRACKED;
	lastUpdate = -1;
	colorText.allocate(IMAGE_WIDTH, IMAGE_HEIGHT, GL_RGB);
	startEngine = false;
	return hr;
}

void testApp::processColor()
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

void testApp::processSkeleton()
{
	NUI_SKELETON_FRAME skeletonFrame = {0};

	pKinectSensor->NuiSkeletonGetNextFrame(0, &skeletonFrame);

	long currentTimeMilliseconds = -1;

	//skeletonFrame.liTimeStamp
	pKinectSensor->NuiTransformSmooth(&skeletonFrame, NULL);
	ofSetBackgroundColor(0,0,0);
			
	skeleton = NUI_SKELETON_DATA();
	skeleton.eTrackingState = NUI_SKELETON_NOT_TRACKED;

	for (int i = 0; i < NUI_SKELETON_COUNT; ++i)
  {
		if(skeletonFrame.SkeletonData[i].eTrackingState == NUI_SKELETON_TRACKED)
		{
			skeleton = skeletonFrame.SkeletonData[i];
			currentTimeMilliseconds = skeletonFrame.liTimeStamp.LowPart;
			break;
		}
	}

	if(skeleton.eTrackingState == NUI_SKELETON_TRACKED && startEngine)
	{
		double deltaTimeMilliseconds = (currentTimeMilliseconds - lastUpdate);
		if (lastUpdate == -1)
			deltaTimeMilliseconds = 0;
		lastUpdate = currentTimeMilliseconds;

		SkeletonData measuredArms = SkeletonData();
		measuredArms.RightShoulderCms = convert(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_RIGHT]);
		measuredArms.RightElbowCms		= convert(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_ELBOW_RIGHT]);
		measuredArms.RightWristCms		= convert(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_WRIST_RIGHT]);
		measuredArms.RightHandCms			= convert(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_HAND_RIGHT]);

		measuredArms.LeftShoulderCms	= convert(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_LEFT]);
		measuredArms.LeftElbowCms			= convert(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_ELBOW_LEFT]);
		measuredArms.LeftWristCms			= convert(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_WRIST_LEFT]);
		measuredArms.LeftHandCms			= convert(skeleton.SkeletonPositions[NUI_SKELETON_POSITION_HAND_LEFT]);

		ArmFatigueUpdate update = engineCE->ProcessNewSkeletonData(measuredArms, deltaTimeMilliseconds / 1000.00);
		labelCE = ofToString(update.RightArm.ConsumedEndurance, 2);
	}

}

Vector3D testApp::convert(Vector4 trackedPoint)
{
			Vector3D jointPos;
			jointPos.X = trackedPoint.x;
			jointPos.Y = trackedPoint.y;
			jointPos.Z = trackedPoint.z;
			return jointPos;
}

void testApp::drawSkeleton(const NUI_SKELETON_DATA & skel)
{
	if(skel.eTrackingState != NUI_SKELETON_TRACKED)
		return;
		
	// Render Torso
  drawBone(skel, NUI_SKELETON_POSITION_HEAD, NUI_SKELETON_POSITION_SHOULDER_CENTER);
  drawBone(skel, NUI_SKELETON_POSITION_SHOULDER_CENTER, NUI_SKELETON_POSITION_SHOULDER_LEFT);
  drawBone(skel, NUI_SKELETON_POSITION_SHOULDER_CENTER, NUI_SKELETON_POSITION_SHOULDER_RIGHT);
  drawBone(skel, NUI_SKELETON_POSITION_SHOULDER_CENTER, NUI_SKELETON_POSITION_SPINE);
  drawBone(skel, NUI_SKELETON_POSITION_SPINE, NUI_SKELETON_POSITION_HIP_CENTER);
  drawBone(skel, NUI_SKELETON_POSITION_HIP_CENTER, NUI_SKELETON_POSITION_HIP_LEFT);
  drawBone(skel, NUI_SKELETON_POSITION_HIP_CENTER, NUI_SKELETON_POSITION_HIP_RIGHT);

  // Left Arm
  drawBone(skel, NUI_SKELETON_POSITION_SHOULDER_LEFT, NUI_SKELETON_POSITION_ELBOW_LEFT);
  drawBone(skel, NUI_SKELETON_POSITION_ELBOW_LEFT, NUI_SKELETON_POSITION_WRIST_LEFT);
  drawBone(skel, NUI_SKELETON_POSITION_WRIST_LEFT, NUI_SKELETON_POSITION_HAND_LEFT);

  // Right Arm
  drawBone(skel, NUI_SKELETON_POSITION_SHOULDER_RIGHT, NUI_SKELETON_POSITION_ELBOW_RIGHT);
  drawBone(skel, NUI_SKELETON_POSITION_ELBOW_RIGHT, NUI_SKELETON_POSITION_WRIST_RIGHT);
  drawBone(skel, NUI_SKELETON_POSITION_WRIST_RIGHT, NUI_SKELETON_POSITION_HAND_RIGHT);

  // Left Leg
  drawBone(skel, NUI_SKELETON_POSITION_HIP_LEFT, NUI_SKELETON_POSITION_KNEE_LEFT);
  drawBone(skel, NUI_SKELETON_POSITION_KNEE_LEFT, NUI_SKELETON_POSITION_ANKLE_LEFT);
  drawBone(skel, NUI_SKELETON_POSITION_ANKLE_LEFT, NUI_SKELETON_POSITION_FOOT_LEFT);

  // Right Leg
  drawBone(skel, NUI_SKELETON_POSITION_HIP_RIGHT, NUI_SKELETON_POSITION_KNEE_RIGHT);
  drawBone(skel, NUI_SKELETON_POSITION_KNEE_RIGHT, NUI_SKELETON_POSITION_ANKLE_RIGHT);
  drawBone(skel, NUI_SKELETON_POSITION_ANKLE_RIGHT, NUI_SKELETON_POSITION_FOOT_RIGHT);
}

void testApp::drawBone(const NUI_SKELETON_DATA & skel, NUI_SKELETON_POSITION_INDEX joint0, NUI_SKELETON_POSITION_INDEX joint1)
{
	ofPolyline pol = ofPolyline();
	NUI_SKELETON_POSITION_TRACKING_STATE joint0State = skel.eSkeletonPositionTrackingState[joint0];
  NUI_SKELETON_POSITION_TRACKING_STATE joint1State = skel.eSkeletonPositionTrackingState[joint1];
	if (joint0State == NUI_SKELETON_POSITION_NOT_TRACKED || joint1State == NUI_SKELETON_POSITION_NOT_TRACKED)
		return;

	ofPoint point0 = skeletonToScreen(skel.SkeletonPositions[joint0], IMAGE_WIDTH, IMAGE_HEIGHT);
	ofPoint point1 = skeletonToScreen(skel.SkeletonPositions[joint1], IMAGE_WIDTH, IMAGE_HEIGHT);
	pol.addVertex(point0);
	pol.addVertex(point1);

	pol.draw();
}

ofPoint testApp::skeletonToScreen(Vector4 skeletonPoint, const int width, const int height)
{
	LONG x, y;
  USHORT depth;

  NuiTransformSkeletonToDepthImage(skeletonPoint, &x, &y, &depth);
	float screenPointX = static_cast<float>(x * width) / 320;
  float screenPointY = static_cast<float>(y * height) / 240;
	return ofPoint(screenPointX, screenPointY);
}

void testApp::startButtonClick()
{
	startEngine = true;
	engineCE->Reset();
}

void testApp::stopButtonClick()
{
	startEngine = false;
}

//--------------------------------------------------------------
void testApp::update()
{
	if(pKinectSensor == NULL)
		return;
	if (WAIT_OBJECT_0 == WaitForSingleObject(colorEvent, 0))
		processColor();
	if ( WAIT_OBJECT_0 == WaitForSingleObject(skeletonEvent, 0) )
		processSkeleton();
}

void testApp::stopKinect()
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

void testApp::exit()
{
	stopKinect();

	startButton.removeListener(this, &testApp::startButtonClick);
	stopButton.removeListener(this, &testApp::stopButtonClick);
}

//--------------------------------------------------------------
void testApp::draw()
{
	ofSetColor(ofColor::white);
	colorText.draw(0, 0);

	ofSetColor(ofColor::green);
	ofSetLineWidth(SKELETON_STROKE);
	drawSkeleton(skeleton);
	ofSetColor(ofColor::black);
	ofRect(0, IMAGE_HEIGHT, 0, IMAGE_WIDTH, 50);
	startButton.draw();
	stopButton.draw();

	labelCE.draw();
}		

testApp::testApp(ofAppGlutWindow *win)
{
	window = win;
	window->setWindowShape(WINDOW_WIDTH, WINDOW_HEIGHT);
	window->setWindowTitle("Arm Consumed Endurance (CE)");
}

//--------------------------------------------------------------
void testApp::keyPressed(int key)
{

}

//--------------------------------------------------------------
void testApp::keyReleased(int key)
{

}

//--------------------------------------------------------------
void testApp::mouseMoved(int x, int y )
{
}

//--------------------------------------------------------------
void testApp::mouseDragged(int x, int y, int button)
{
}

//--------------------------------------------------------------
void testApp::mousePressed(int x, int y, int button)
{
}

//--------------------------------------------------------------
void testApp::mouseReleased(int x, int y, int button)
{
}

//--------------------------------------------------------------
void testApp::windowResized(int w, int h)
{
	if(w != WINDOW_WIDTH || h !=WINDOW_HEIGHT)
		window->setWindowShape(WINDOW_WIDTH, WINDOW_HEIGHT);
}

//--------------------------------------------------------------
void testApp::gotMessage(ofMessage msg)
{
}

//--------------------------------------------------------------
void testApp::dragEvent(ofDragInfo dragInfo)
{ 
}