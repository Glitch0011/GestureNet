# GestureNet

## Load Gestures from JSON file

Gestures can be stored as an array in a single JSON file.

`var trainingSet = GestureLoader.ReadGesture(new FileInfo("data.bin")).ToList();`

## Analyse a Gesture

Returns a list of possible Gestures in order of likelyhood.

`var guess = PointCloudRecognizer.Classify(new Gesture(points), trainingSet);`

## Add a new Gesture

Add's a pre-normalised Gesture to the training set.

`trainingSet.Add(new Gesture(points, textBox1.Text));`

## Save Gestures

Saves all the Gestures into a single JSON file.

`GestureLoader.SaveGestures(new FileInfo("data.bin"), trainingSet);`
