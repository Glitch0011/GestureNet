# GestureNet

## Analyse a Gesture

```
trainingSet = GestureLoader.ReadGesture(new FileInfo("data.bin")).ToList();

var guess = PointCloudRecognizer.Classify(new Gesture(points), trainingSet);
```

## Add a new Gesture

`trainingSet.Add(new Gesture(points, textBox1.Text));`

## Save Gestures

`GestureLoader.SaveGestures(new FileInfo("data.bin"), trainingSet);`
