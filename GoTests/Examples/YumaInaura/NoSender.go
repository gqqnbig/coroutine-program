package main

func main() {
	messages := make(chan string)

	// Do nothing spawned goroutine
	go func() {}()

	// A groutine ( main groutine ) trying to receive message from channel
	// But channel has no messages, it is empty.
	// And no other groutine running. ( means no "Sender" exists )
	// So channel will be deadlocking
	<-messages // fatal error: all goroutines are asleep - deadlock!
}