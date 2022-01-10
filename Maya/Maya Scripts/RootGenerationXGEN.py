import maya.cmds as cmds

object = cmds.ls(orderedSelection = True)[0]


xgen.createPalette(
xgen.createDescription('Xgen', 'test_' , object, 'none','Arnold','default')