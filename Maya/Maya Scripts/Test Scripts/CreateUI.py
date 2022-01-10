#KeyRotationWithUI.py

import maya.cmds as cmds
import functools

def CreateUI(pWindowTitle,pApplyCallback):
	windowID = 'myWindowID'
	
	if(cmds.window(windowID,exists=True)):
		cmds.deleteUI(windowID)
		
	cmds.window(windowID, title = pWindowTitle,sizeable=False,resizeToFitChildren=True)
	
	#cmds.rowColumnLayout(numberOfColumns=3,columnOffset = [(1,'right',3)])
	cmds.gridLayout( numberOfColumns = 3, cellWidthHeight = (60,20))
	#cmds.rowColumnLayout(numberOfColumns=3,columnWidth =(1.75),columnWidth =(2,60),columnOffset = [(1,'right',3)])
	#cmds.rowColumnLayout(columnWidth = (1,75))
	#cmds.rowColumnLayout(columnWidth = (2,60))
	#cmds.rowColumnLayout(columnWidth = (3,60))
	
	cmds.text( label = 'Time range:') 
	startTimeField = cmds.intField(value = cmds.playbackOptions(query=True, minTime=True))
	endTimeField = cmds.intField(value = cmds.playbackOptions(query=True, maxTime = True))
	
	cmds.text( label = 'Attribute:') 
	targetAttributeField = cmds.textField(text = 'rotateY')	
	cmds.separator(h=10,style='none')
	
	cmds.separator(h=10,style='none')
	cmds.separator(h=10,style='none')
	cmds.separator(h=10,style='none')
	
	cmds.separator(h=10,style='none')
	cmds.button(label= 'Apply', command = functools.partial(pApplyCallback,
													startTimeField,
													endTimeField,
													targetAttributeField))
	
	
	def CancelCallback (*pArgs):
		if(cmds.window(windowID, exists = True)) :
			cmds.deleteUI(windowID)
	
	cmds.button(label= 'Cancel', command = CancelCallback)
		
	cmds.showWindow()
	
def KeyFullRotation (pObjectName, pStartTime,pEndTime,pTargetAttribute):
	
	cmds.cutKey(pObjectName,time = (pStartTime,pEndTime), attribute = pTargetAttribute)
		
	cmds.setKeyframe(pObjectName,time = pStartTime, attribute = pTargetAttribute,value=0)
	cmds.setKeyframe(pObjectName,time = pEndTime, attribute = pTargetAttribute,value=360)
	cmds.selectKey(pObjectName,time=(pStartTime,pEndTime),attribute = pTargetAttribute,keyframe=True)
	cmds.keyTangent( inTangentType = 'linear', outTangentType = 'linear') 

def ApplyCallback(pStartTimeField,pEndTimeField,pTargetAttributeField ,*pArgs):
	
	startTime = cmds.intField(pStartTimeField,query = True, value = True)
	endTime = cmds.intField(pEndTimeField,query = True, value = True)
	targetAttribute = cmds.textField(pTargetAttributeField, query=True, text = True)

	print('start time: %s' % (startTime))
	print('end time: %s' % (endTime))
	print('Target Attribute: %s' % (targetAttribute))
	
	selectionList = cmds.ls( selection=True, type = 'transform')
	
	for objectName in selectionList:
		KeyFullRotation(objectName,startTime,endTime, targetAttribute)




CreateUI('My title', ApplyCallback)
	
	
	
	 