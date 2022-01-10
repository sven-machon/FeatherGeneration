#keyRotation.py

import maya.cmds as cmds

selectionList = cmds.ls(selection=True)

if len(selectionList)>=1:
	
	for objectName in selectionList:
		objectTypeResult = cmds.objectType(objectName)
		
		print('%s type: %s' % (objectName,objectTypeResult))
else:
	print('Please select at least one object')