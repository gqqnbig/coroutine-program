package main

import "fmt"

var ch1 chan int = make(chan int)
var ch2 chan bool = make(chan bool)

func s(v int) {
	defer func() {
		defer d()
		<-ch2
	}()
	ch1 <- v
}

func d() {
	ch2 <- true
}

func main() {
	go s(2)

	v1 := <-ch1
	ch2 <- false
	v2 := <-ch2
	fmt.Println(v1, v2)
}

