package main

func main() {
	messages := make(chan string)

	// Do nothing spawned goroutine
	go func() {}()

	// A groutine ( main groutine ) trying to send message to channel
	// But no other groutine runnning
	// And channel has no buffers
	// So it raises deadlock error
	messages <- "I wanna tell you." // fatal error: all goroutines are asleep - deadlock!
}