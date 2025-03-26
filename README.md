# IL-ARLPCG

## Welcome! 
This Unity Implementation of the ARLPCG Theorised and Presented by SEED (Electronic Arts' Machine Learning R&D Company)

This is a continutation of the research to by adding in imitation learning into the process to speed up training times for the adversarial agents.

[Check Out the Original Presentation and Paper Here](https://www.ea.com/seed/news/cog2021-adversarial-rl-content-generation)

[For a quick summary, watch a 5-min explanation video](https://www.youtube.com/watch?v=DcBS5_sZu2M)

##### **If you're just here for a browse:**
1. All custom code for the project is found in 'Assets -> Scripts'.
2. A portion of this project uses the ML-Agents library for Unity, this connects the Unity runtime via socket connection to the RL models training and setup in Python.
3. This project was made to be used as an example for anyone who would like to implement their own Adversarial Reinforcement Learning Models.
4. Feel free to use whatever code that you would like and contribute to the repo!

##### **If you'd like to try this out on your Machine:**
1. Follow this guide to install the ml agents python library: https://github.com/Unity-Technologies/ml-agents/blob/release_19_docs/docs/Installation.md#advanced-local-installation-for-development.
2. Ensure that Unity and Unity Hub are both installed.
3. Once the project folder is unzipped, click "Open" in Unity hub's main window.
4. Then navigate back to the project folder and select the project folder.
5. This should begin to reinitialize the project.
6. After this is done and the project window is open, navigate to the /Assets where all of the scripts, scenes, materials and prefabs can be seen.
7. Then navigate into the /Scenes folder from /Assets.
8. Double click on the SampleScene. This should display the project scene containing all of the correct objects.
9. You can click play and play the game itself with the WASD and SPACE keys.
10. Or you can access your console and type "mlagents-learn --run-id=<ANY ID YOU WANT>" and press enter.
  Once you have pressed enter, go back to the Unity project window and click play, the model should start training on the environment.
