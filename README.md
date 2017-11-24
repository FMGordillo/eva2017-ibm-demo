# EVA 2017 Demostration

Based on [2D Roguelike Tutorial](https://unity3d.com/es/learn/tutorials/s/2d-roguelike-tutorial), we've created a dialog between a troll and you, the player, using [Watson Conversation](https://www.ibm.com/watson/services/conversation/).
The mission? Save a princess. How? Solving three riddles. Why? Because it's fun, I guess.

## Getting started
If you don't mind using this project, instead of starting from scratch, you need to build your Watson Conversation first.

### Prerequisites
- Have an [IBM Cloud account](https://console.bluemix.net/registration/) (it's free for 30 days, and after you can use certain services for free, A.K.A. Lite, for example Watson Conversation). For more information, [click here](https://www.ibm.com/watson/services/conversation/)

### Steps
After you've created your Watson service, we will go to the [Watson Conversation Dashboard](https://watson-conversation.ng.bluemix.net)

Alternatively, if you want to know the IBM Cloud platform, you can follow this steps (it will take you to the same place): Proceed on your [IBM Cloud dashboard](https://console.bluemix.net/dashboard/apps/), search for your Watson Conversation service, click it, and click on Lauch Tool.

That's the main dashboard *for non-developers and developers* to create the dialog that we will consume.

- Create a workspace ([and what is that?](https://console.bluemix.net/docs/services/conversation/configure-workspace.html#configuring-a-conversation-workspace) or, in other words, our instante of Watson for this game).
- Import [these Intents and these Entities](https://gist.github.com/FMGordillo/9bd63e28ec9ecffb3910ee923cee941e) (and what is an [Intent](https://console.bluemix.net/docs/services/conversation/intents.html#defining-intents)? And an [Entity](https://console.bluemix.net/docs/services/conversation/entities.html#defining-entities)? In short, Intent is the What, and Entity is the Who).
- [Create the Dialog flow](https://console.bluemix.net/docs/services/conversation/dialog-overview.html#dialog-overview).

We gave to you the tools and documentation to start developing your dialog flow. Now it's your turn to start! You can test it on the right top side of your screen, before deploying on your game.

When you fell comfortable with what you've created, go to `Assets/Scripts/GameManager.cs` and edit the Watson variables using your service credentials (you can get it on the [IBM Cloud dashboard](https://console.bluemix.net/dashboard/apps/)).

## Contributing
We welcome all the love that you could give us! Please read [CONTRIBUTING](https://github.com/FMGordillo/eva2017-ibm-demo/blob/master/CONTRIBUTING.md) for more information about the process for submitting pull requests.

## Authors
- Facundo Martin Gordillo - *Joiner and speaker* - ([Twitter](https://www.twitter.com/FMGordillo), [Linkedin](www.linkedin.com/in/fmgordillo))
- Diego Angel Masini - *UI advisor and helper* - ([Twitter](https://twitter.com/diegomasini), [Linkedin](https://www.linkedin.com/in/diegomasini/))
- Nicolas Finamore - *Initial work* - ([Twitter](https://twitter.com/Nicolasie_), [Linkedin](https://www.linkedin.com/in/nicolasie/))

## License
This project is licensed under the Apache License 2.0 - see the [LICENSE.md](https://github.com/FMGordillo/eva2017-ibm-demo/blob/master/LICENSE) file for details

## TODOs
- Persist on [Cloudant](https://www.ibm.com/cloud/cloudant) all dialogs for future analytics (main code is already there, we need to insert a dependency to handle JSONs).
- Make a personality analysis (using [this service](https://www.ibm.com/watson/services/personality-insights/), also Lite) for an after-game feedback (?)
- UI fix for player movement
- Make the princess actually appear!
