import maya.cmds as cmds

selectionList = cmds.ls(orderedSelection = True, type = 'transform')  

if len(selectionList)>=2:
	target = selectionList[0]
	selectionList.remove(target)
	
	locatorGroup  = cmds.group( empty = True, name = 'expansion_locator_grp#')
		
	maxExpansion=100
	newAttributeName = 'expansion'
	
	if not cmds.objExists('%s.%s' % (target, newAttributeName)):
		cmds.select(target) 
		cmds.addAttr(	longName = newAttributeName, shortName = 'exp',
						attributeType = 'double', min =0, max = maxExpansion,
						defaultValue = maxExpansion, keyable=True) 
						
	

	
	for objectName in selectionList:
		tempResult = cmds.getAttr('%s.translate' %(objectName)) 		
		coords = tempResult[0]
		
		locator = cmds.spaceLocator(position = coords, name='%s_loc#' %(objectName))[0]
		cmds.xform(locator ,centerPivots=True)
		cmds.parent(locator, locatorGroup)
		
		pointConstraint= cmds.pointConstraint([target,locator],objectName,name = '%s_pointConstraint#' %(objectName))[0]
		
		cmds.expression(	alwaysEvaluate = True, name = '%s_attractWeight' % (objectName),   
							object = pointConstraint,
							string = '%sW0=%s-%s.%s' %(target,maxExpansion,target,newAttributeName))
		cmds.connectAttr( '%s.%s' %(target, newAttributeName),'%s.%sW1' % (pointConstraint,locator))
		
	cmds.xform(locatorGroup,centerPivots=True)
	
else:
	print('Please select 2 objects')